using Captovix;
int checks=0;
void Check(bool value,string name){if(!value)throw new Exception(name);Console.WriteLine("PASS "+name);checks++;}
DateTimeOffset Created(int month,int day)=>new(new DateTime(2026,month,day,10,15,0));
Directory.CreateDirectory(App.DataRoot);
try{
    var settings=new LibraryRetentionSettings{Enabled=true,Days=60};
    Check(LibraryRetentionSettings.DueDate(Created(11,1),60)==new DateTime(2026,12,31),"01 November +60 days =31 December");
    Check(LibraryRetentionSettings.DueDate(Created(5,1),60)==new DateTime(2026,6,30),"01 May +60 days =30 June");
    Check(settings.IsDue(Created(5,1),new(2026,11,20)),"Old files become eligible after enabling");
    Check(!settings.IsDue(Created(11,1),new(2026,12,30)),"Never delete before expiry day");
    Check(settings.IsDue(Created(11,1),new(2026,12,31)),"Eligible on expiry date");
    Check(!(settings with{Enabled=false}).IsDue(Created(5,1),new(2026,11,20)),"Disabled keeps all files");
    Check(LibraryRetentionSettings.NextRun(new(2026,11,20,12,0,0))==new DateTime(2026,11,20,23,59,0),"Schedule at23:59 today");
    Check(LibraryRetentionSettings.NextRun(new(2026,11,20,23,59,0))==new DateTime(2026,11,21,23,59,0),"No continuous cleanup after daily deadline");
    Check(settings.ScheduledRun(new(2026,11,20,9,0,0))==new DateTime(2026,11,19,23,59,0),"Opening app catches latest missed deadline");
    Check((settings with{LastRunDate=new(2026,11,19)}).ScheduledRun(new(2026,11,20,9,0,0))==new DateTime(2026,11,20,23,59,0),"Completed catch-up does not run again");
    Check((settings with{EnabledAt=new(2026,11,20,8,0,0)}).ScheduledRun(new(2026,11,20,9,0,0))==new DateTime(2026,11,20,23,59,0),"First enable waits for first deadline");
    Check(settings.IsDue(Created(5,1),new DateTime(2026,11,19))&&!settings.IsDue(Created(11,1),new DateTime(2026,11,19)),"Catch-up includes expired items only");
    // Regression for V1.68.1 startup crash on 25 Nov 2026 at 08:19.
    var startup=new DateTime(2026,11,25,8,19,56);
    var missed=settings.ScheduledRun(startup);
    Check(missed==new DateTime(2026,11,24,23,59,0),"Keep missed deadline as expiry cutoff");
    Check(LibraryRetentionSettings.TimerDelay(missed,startup)==TimeSpan.FromSeconds(15),"Missed deadline yields safe startup delay");
    Check(LibraryRetentionSettings.TimerDelay(startup,startup)==TimeSpan.FromSeconds(15),"Exact deadline yields safe delay");
    Check(LibraryRetentionSettings.TimerDelay(startup.AddTicks(-1),startup)==TimeSpan.FromSeconds(15),"Past one tick never reaches DispatcherTimer");
    Check(LibraryRetentionSettings.TimerDelay(startup.AddTicks(1),startup)==TimeSpan.FromTicks(1),"Future one tick stays positive");
    var afterCatchup=settings with{LastRunDate=missed.Date};
    var next=afterCatchup.ScheduledRun(startup);
    Check(next==new DateTime(2026,11,25,23,59,0)&&LibraryRetentionSettings.TimerDelay(next,startup)==next-startup,"After catch-up schedule next daily run");
    var atDeadline=new DateTime(2026,11,25,23,59,0);
    Check(settings.ScheduledRun(atDeadline)==atDeadline&&LibraryRetentionSettings.TimerDelay(atDeadline,atDeadline)==TimeSpan.FromSeconds(15),"Exactly 23:59 catches up without zero interval");
    Check(!settings.IsDue(Created(9,26),missed)&&settings.IsDue(Created(9,25),missed),"Catch-up cutoff does not delete today's newly expired content early");
    Check(LibraryRetentionSettings.TimerDelay(next,startup.AddHours(-2))==next-startup.AddHours(-2),"Clock moved back keeps future wait positive");
    Check(LibraryRetentionSettings.TimerDelay(next,next.AddHours(2))==TimeSpan.FromSeconds(15),"Clock moved forward gives safe catch-up delay");
    settings.Save(App.DataRoot);Check(LibraryRetentionSettings.Load(App.DataRoot)==settings,"Configuration survives restart");
    string file=Path.Combine(App.DataRoot,"original.png");File.WriteAllText(file,"pixel bytes");
    var item=new MediaItem{Id="one",FilePath=file};var store=new MediaStore();store.Items.Add(item);store.Save();
    var batch=RetentionBatch.Stage([item],[file]);
    Check(!File.Exists(file)&&File.Exists(batch.Entries[0].Stored),"Staging preserves original bytes");
    Check(File.Exists(Path.Combine(App.DataRoot,"retention-pending.json")),"Journal exists before metadata commit");
    RetentionBatch.Recover(store);Check(File.ReadAllText(file)=="pixel bytes"&&store.Items.Count==1,"Recover interrupted move before commit");
    batch=RetentionBatch.Stage([item],[file]);store.Items.Clear();store.Save();batch=batch with{Committed=true};batch.Save();
    RetentionBatch.Recover(store);Check(File.ReadAllText(file)=="pixel bytes"&&store.Items.Single().Id=="one","Recover interrupted recycle after metadata commit");
    batch=RetentionBatch.Stage([item],[file]);store.Items.Clear();store.Save();batch=batch with{Committed=true};batch.Save();
    batch.Rollback(false); // Crash after files restored but before library restored.
    RetentionBatch.Recover(store);Check(File.ReadAllText(file)=="pixel bytes"&&store.Items.Single().Id=="one","Recover crash during rollback keeps original ID");
    batch=RetentionBatch.Stage([item],[file]);batch=batch with{Committed=true};batch.Save();
    // Simulate the folder having successfully left the filesystem for the Windows Recycle Bin.
    string recycled=Path.Combine(App.DataRoot,"simulated-recycle");Directory.Move(batch.Folder,recycled);
    RetentionBatch.Recover(store);Check(store.Items.Count==0,"Recovery completes metadata after successful recycle");
    Check(File.ReadAllText(Path.Combine(recycled,"0-original.png"))=="pixel bytes","Recycle transaction does not alter bytes");
    Console.WriteLine($"{checks} retention checks passed");
}finally{Directory.Delete(App.DataRoot,true);}

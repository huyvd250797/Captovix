"""Read raw ICO images and Windows PE RT_ICON resources with Python stdlib."""
import struct
def ico_images(data):
 count=struct.unpack_from('<H',data,4)[0];result=[]
 for i in range(count):
  size,offset=struct.unpack_from('<II',data,6+16*i+8);result.append(data[offset:offset+size])
 return result
def pe_icons(data):
 pe=struct.unpack_from('<I',data,60)[0]
 if data[pe:pe+4]!=b'PE\0\0':raise ValueError('Not a Windows PE')
 count=struct.unpack_from('<H',data,pe+6)[0];opt_size=struct.unpack_from('<H',data,pe+20)[0];opt=pe+24;magic=struct.unpack_from('<H',data,opt)[0]
 directory=opt+(112 if magic==0x20b else 96);resource_rva=struct.unpack_from('<I',data,directory+16)[0];sections=[]
 for i in range(count):
  at=opt+opt_size+i*40;virtual_size,virtual_at,raw_size,raw_at=struct.unpack_from('<IIII',data,at+8);sections.append((virtual_at,max(virtual_size,raw_size),raw_at))
 def offset(rva):
  for base,size,raw in sections:
   if base<=rva<base+size:return raw+rva-base
  raise ValueError('Unmapped resource RVA')
 root=offset(resource_rva)
 def entries(relative):
  at=root+relative;named,ids=struct.unpack_from('<HH',data,at+12)
  return [struct.unpack_from('<II',data,at+16+8*i) for i in range(named+ids)]
 result=[]
 def walk(relative):
  for name,child in entries(relative):
   if child&0x80000000:walk(child&0x7fffffff)
   else:
    rva,size=struct.unpack_from('<II',data,root+child);at=offset(rva);result.append(data[at:at+size])
 for name,child in entries(0):
  if name==3:walk(child&0x7fffffff)
 return result

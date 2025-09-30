alias LCD d0
move r0 0
s d0 Mode 0
s d0 Setting r0
add r0 r0 1
mod r0 r0 3600
yield
j 3
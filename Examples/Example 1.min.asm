alias LCD d0
s d0 Mode 10
s d0 Setting 310939249775
yield
j 3
move r0 0
s d0 Mode 0
s d0 Setting r0
add r0 r0 1
mod r0 r0 3600
yield
j 7
jr 1
jr 0
select r14 r0 258977646164 90474682012734
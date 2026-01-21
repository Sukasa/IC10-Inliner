macro testloop param1
scope_label:
yield
add param1 param1 2
j scope_label
endmacro

testloop r0
add r1 r1 1
testloop r2
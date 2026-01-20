macro test_loop param1
scope_label:
yield
add param1 param1 2
j scope_label
endmacro

test_loop r0
add r1 r1 1
test_loop r2
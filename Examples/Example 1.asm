# This is an example assembly file, showing what the IC10 inliner can do
# For this example, the inliner was called with IC10 inliner.exe "Example 1.asm" -s incrementing_number

section definitions
  alias LCD d0
  alias Counter r0
  
  define MyString       STR("Hello")
  define Simple         0
  define Text           10
	
section show_string requires definitions
  s LCD Mode Text
  s LCD Setting MyString

wait_loop:
  yield
  j wait_loop

section incrementing_number requires definitions
  move Counter 0
  s LCD Mode Simple

counter_wait_loop:
  s LCD Setting Counter
  add Counter Counter 1
  mod Counter Counter 3600
  yield
  j counter_wait_loop
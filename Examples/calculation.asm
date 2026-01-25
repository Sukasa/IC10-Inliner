  define  SP_TABLE_START  20
  define  SP_TABLE_ENTRY_STRIDE 10

macro TableEntry Num Value1 Value2
  poke Calc(SP_TABLE_ENTRY_STRIDE*Num+SP_TABLE_START) Value1
  poke Calc(SP_TABLE_ENTRY_STRIDE*Num+SP_TABLE_START+1) Value1
endmacro

  TableEntry 0 11 22
  TableEntry 2 33 44
macro SWOZZLE _dst _a _b
	add _dst _dst _a
	SWIZZLE _dst _b
endmacro

macro SWIZZLE _dst _src
	xor _dst _dst _src
	sla _dst _dst 4
	xor _dst _dst _src
endmacro

	move r1 0
	move r2 18
	SWIZZLE r1 r2
	SWOZZLE r1 200 512
	SWIZZLE r1 48

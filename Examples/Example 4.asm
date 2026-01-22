
define ThisIsDefined 1
;define ThisAint

; ############################################
; Expected output: r0 with 1,4

ifdef ThisIsDefined
	move r0 1
endif

ifdef ThisAint
	move r0 2
endif

ifndef ThisIsDefined
	move r0 3
endif

ifndef ThisAint
	move r0 4
endif


; ############################################
; Expected output: r1 with 1,3,4
; 2,6,7 should not be output

ifdef ThisIsDefined ; enabled
	move r1 1

	ifdef ThisAint ; disabled
		move r1 2
	endif

	ifndef ThisAint ; enabled
		move r1 3
	endif
endif

ifndef ThisAint ; enabled
	move r1 4

	ifndef ThisIsDefined ; disabled
		move r1 5
	endif
endif

ifdef ThisAint ; disabled
	move r1 6

	ifdef ThisIsDefined ; disabled (by parent disable)
		move r1 7
	endif
endif

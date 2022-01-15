;Project: High Pass Filter for bitMaps.
;Author: Kacper Nitkiewicz, Informatyka, year 3, sem. 5, gr. 5, date: 15.01.2021
;Vesion: 1.0.

.DATA
MaskMatrix BYTE 1, -2, 1, -2, 5, -2, 1, -2, 1 ;0, -1, 0, -1, 5, -1, 0, -1, 0

.CODE

DllEntry PROC hInstDLL:DWORD, reason:DWORD, reserved1:DWORD

mov	eax, 1
ret

DllEntry ENDP

ApplyMaskToRGB proc

ApplyMaskToRGB_Start:
	pxor xmm5, xmm5
	pxor xmm4, xmm4
	pxor xmm3, xmm3
	movq xmm5, QWORD PTR [MaskMatrix]
	pmovsxbw xmm3, xmm5
	movq xmm5, QWORD PTR [RCX]
	pmovzxbw xmm4, xmm5
	pmaddwd xmm3, xmm4
	phaddd xmm3, xmm3
	phaddd xmm3, xmm3
	xor RAX, RAX
	movd EAX, xmm3
	movsxd RAX, EAX
	movsx RBX, BYTE PTR [MaskMatrix + 8]
	movzx RDX, BYTE PTR [RCX + 8]
	imul RBX, RDX
	add RAX, RBX

ApplyMaskToRGB_End:
	movq xmm14, RAX
	mov RBX, 0
	movq xmm15, RBX
	maxpd xmm14, xmm15
	mov RBX, 255
	movq xmm15, RBX
	minpd xmm14, xmm15
	movq RAX, xmm14

	ret

ApplyMaskToRGB endp

ApplyHighPassFilterAsm proc

ApplyHighPassFilterAsm_Begin:

	xor R10, R10
	cvtsi2sd xmm0, RDX
	cvtsi2sd xmm1, R8
	cvtsi2sd xmm2, R9
	mov R11, QWORD PTR [RSP + 40]
	mov RDX, QWORD PTR [RSP + 48]
	inc R11	
	sub R11, R9
	mov R8, RCX
	mov R9, RDX
	cvtsd2si R11, xmm2

ApplyHighPassFilterAsm_For1:

	mov R12, R11
	cvtsd2si R13, xmm1

	cmp R12, R13
	jl ApplyHighPassFilterAsm_Continue

	mov RAX, R11
	xor RDX, RDX
	cvtsd2si RBX, xmm1
	div RBX

	cmp RDX, 0
	je ApplyHighPassFilterAsm_Continue

	cvtsd2si RBX, xmm0 
	cvtsd2si RCX, xmm1
	sub RBX, RCX
	cmp R12, RBX
	jge ApplyHighPassFilterAsm_Continue

	mov RAX, R11
	add RAX, 3
	xor RDX, RDX
	cvtsd2si RBX, xmm1
	div RBX

	cmp RDX, 0	
	je ApplyHighPassFilterAsm_Continue

	xor R13, R13

ApplyHighPassFilterAsm_For2:

	xor R14, R14
	cmp R13, 3
	je ApplyHighPassFilterAsm_EndFor
	jmp ApplyHighPassFilterAsm_For3

ApplyHighPassFilterAsm_For3:

	mov RBX, R14
	dec RBX
	imul RBX, 3

	mov RAX, R13
	dec RAX
	cvtsd2si RCX, xmm1
	imul RAX, RCX

	add RBX, RAX
	add RBX, R12

	mov RCX, R13
	imul RCX, 3
	add RCX, R14

	mov R15B, BYTE PTR [R8 + RBX]
	mov RAX, QWORD PTR [RSP + 56]

	mov BYTE PTR [RAX + RCX], R15B

	inc RBX	
	mov R15B, BYTE PTR [R8 + RBX]
	mov RAX, QWORD PTR [RSP + 64]

	mov BYTE PTR [RAX + RCX], R15B

	inc RBX	
	mov R15B, BYTE PTR [R8 + RBX]
	mov RAX, QWORD PTR [RSP + 72]

	mov BYTE PTR [RAX + RCX], R15B 

	inc R14
	cmp R14, 3
	jne ApplyHighPassFilterAsm_For3
	inc R13
	jmp ApplyHighPassFilterAsm_For2

ApplyHighPassFilterAsm_EndFor:

	mov RCX, QWORD PTR [RSP + 56]
	call ApplyMaskToRGB

	mov RDX, R11
	cvtsd2si RCX, xmm2
	sub RDX, RCX

	mov BYTE PTR [R9 + RDX], AL

	mov RCX, QWORD PTR [RSP + 64]
	call ApplyMaskToRGB

	mov RDX, R11
	cvtsd2si RCX, xmm2
	sub RDX, RCX
	inc RDX

	mov BYTE PTR [R9 + RDX], AL

	mov RCX, QWORD PTR [RSP + 72]
	call ApplyMaskToRGB

	mov RDX, R11
	cvtsd2si RCX, xmm2
	sub RDX, RCX
	add RDX, 2

	mov BYTE PTR [R9 + RDX], AL

	jmp ApplyHighPassFilterAsm_Continue

ApplyHighPassFilterAsm_Continue:

	add R11, 3
	cmp R11, QWORD PTR [RSP + 40]
	jg ApplyHighPassFilterAsm_End
	jmp ApplyHighPassFilterAsm_For1
	
ApplyHighPassFilterAsm_End:

	mov RAX, R9
	ret

ApplyHighPassFilterAsm endp

END
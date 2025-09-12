







GM Seed/Key Algorithms
(En Complètement)























Introduction
In order to communicate with a vehicle power-train control module (PCM) to the level of reprogramming its on-board flash memory, it is necessary to properly unlock the PCM.  This is because PCM’s are protected, such that you request a seed value from the PCM, calculate the corresponding key value via a predetermined algorithm, and send the key to the PCM to unlock it.  The problem in reverse engineering PCM’s is being able to determine the “predetermined algorithm”.  It could be a very complex algorithm, or it could be something as simple as bit inverting.

In the past, we have determined seed/key algorithms by trial and error.  That is, we took the GM hardware/software that calculated the key from a given seed, and feed it a series of seeds.  The seeds chosen covered various bit patterns to help simplify the deduction of an algorithm without having to run all seed values.  After a sufficient number of seed/key pairs were obtained, then we set out to find a mathematical algorithm that described the pairs.  In some cases, this worked.  In others it produced equations that would work for, perhaps, the seed/key pairs we had but would later fail on some small subset of the overall group of possible seed/key combinations.

An attempt to remedy this, and the second method of obtaining seed/key algorithms, was to use debugging tools to step through the GM software that was calculating the seed/key.  By writing down the mathematical steps involved in calculating a particular algorithm, we were able to deduce the algorithm for a number of vehicle platforms, but there were problems.  The problem is that we were only able to use this method for the vehicle PCM’s we had on hand. For future PCM’s, by the time they became available, the GM software had changed such that the process of setting up the debugging tools and single stepping had to be entirely revisited.  However, we did have in our hand 3 or 4 known algorithms that were in the exact form as GM was calculating them.  In other words, while there may be, and often is, more than one mathematical equation that satisfies the seed/key algorithm, we actually had, as a result of the debugging work, the actual exact algorithms for 3-4 of the vehicle platforms.

How It Was Done
It is important to have the exact algorithm, because now we have something to search for in the GM code to find all the other algorithms.  Due to the method of storing things, it was supposed that GM probably kept a large array or matrix of seed/key algorithms.  This came from many factors and reasons.  One of which was the fact that in the debugging work, the algorithms were always calculated in 4 steps and with what seemed to be some special form of a reverse-polish notation.  And secondly, there was a strange number that is always kept as an argument to the seed-request command in the communications script used by the GM programming gear.  The strange number appeared to be an index into some table to denote a particular algorithm.  This seemed to hold because whenever the algorithm would change, so would the number, and only when the algorithm changed would the number change.

The question was: “where is the matrix, if one truly exists?”.  There were basically only two answers to this question.  It could be either in GM’s executable code, or it could be in one of the many data files that the executable code opens and uses.  Since we had a number of actual values from several of the equations, a logical way of searching is to look for those specific numbers.  So, we started with the executable by writing down all occurrences of the numbers from the algorithms.  If they were to exist in any table or data form, chances are they would be clustered together.  Search for them in the executable yielded nothing.  There was no clustering of any of the known equation values.  This meant that if it did exist in a table or matrix format, then it had to be in a data file.

From the past debugging tools used, we were able to capture the filenames of the files that were opened and read/written during the reprogramming process.  So the data file had to be one of those files.  Then it became a matter of examining each of those data files for clustering of known data.  To speed the process, the most likely file was chosen first.  Of the files, only one stood out as being a “most likely” candidate.  And that file was “stgterm.dat” which was located in the \techline\root directory of the GM data CD’s.
The “stgterm.dat” file was examined and low-and-behold, not only was there clustering, but they appeared in the exact order as they were in the known algorithms and the indexes within the file also matched those of the mysterious argument in the request-seed command of the communication scripts.  This means we had found the infamous matrix, but now we had to decode it.

Decoding the Matrix
Since the mysterious argument in the request-seed command, which we will heretofore call the algorithm index, was of only one byte, it made sense to have no more than 256, or less, working algorithms.  It just so happened to be that the “stgterm.dat” file was exactly 13*256 and it soon became apparent that each algorithm entry was 13 bytes long, as each of the known algorithms seemed to be encompassed by its respective 13 byte cluster.  Since we knew the exact numbers from the known algorithms along with their associated operation, it became a matching game of matching operands with an opcode.

It was discovered that 4 opcode/operand pairs or 4 operations existed within each 13-byte entry, which corresponded to the 4 we found in debugging.  And each of these 4 was exactly 3 bytes in length.  Then it became a matter of comparing the opcodes to the mathematical operation previously written down for each of the known algorithms.  It was obvious that we had at least 6 different unique opcodes:

0x14 = Add
0x2A = Complement
0x4C = Rotate Left
0x6B = Rotate Right
0x7E = Flip hi/low bytes and Add
0x98 = Subtract

We thought we had it solved, as it held perfectly for the known algorithms.  So, we took this, looked up the index for another vehicle platform, and figured out its algorithm based on what we knew.  But, it didn’t work.  What was wrong?  Could the 13th byte be used for something?  (There are 12 bytes used for each operation in the algorithm – 4 groups of 3 – meaning there was one extra byte in each algorithm entry).  But, there seemed to be nothing common or discernable from the 13th byte.  We had to do something else.

Looking at the Code Again
We figured that the best way, and only certain way, of determining exactly what each and every operation was, was to go back and look again at the disassembly of the executable code.  Because after all, who was to say that there wasn’t more than just these 6 opcodes?

Finding the part of the executable that contained the algorithm was very easy; all we had to do was look for a call to fopen, fseek, fread, and fclose all appearing closely after some string operation with the string “stgterm.dat”.  This we were certain, because it was evident that the executable was written in C and used standard stdlib and stdio calls.  Two such entries in the executable code meeting this requirement were found and both were found to be running through the algorithm.  It was no surprise, and actually it was verification, that the fseek seeked the location that was 13 times the algorithm index and the fread read exactly 13 bytes.  Thus we knew our grouping was indeed correct.

We soon found that our 6 opcodes were correct and it turns out that only those 6 are valid, others are don’t cares.  So why did our attempt to calculate a new algorithm fail?  Well, it turns out that there are several variants of these 6 opcodes, or I should say stipulations or “tweaks”.  That is, these opcodes weren’t just plain vanilla operations.  Instead, on 2 of the 6 opcodes, the operand is checked and the exact operation is determined as a combination of both.  If we let HHLL represent our 16-bit operand from the data file, where HH is the upper byte from the file (in Motorola byte ordering) and LL is the lower byte, we can write these new operations as follows:

0x14 = Add HHLL
0x2A = Complement – if HH>LL use 2’s complement, else use 1’s complement
0x4C = Rotate Left by HH bits
0x6B = Rotate Right by LL bits
0x7E = Flip hi/low bytes of current calculated key value.  Then if LL>HH add LLHH, else add HHLL.
0x98 = Subtract HHLL
All Others = No-Operation

We made these tweaks to the algorithm we had previously attempted to calculate, and with no surprise, it worked.  However, in looking at the code, we found no reference to the 13th byte.  So it must be that it is used for an entirely different module, or it is for data-integrity testing, or it has nothing to do with anything and exists only as fodder.

Data Format – The File
The filename of the algorithm file, again, is “stgterm.dat” and it is located in the \techline\root directory.  It is accessed and used by the main executable “erdmain.exe” in the \techline\exe directory.  It is possible that other executables use it, but no others are known at this time.

A copy of the “stgterm.dat” is included at the end of this document, formatted in its proper grouping of 13 bytes along with index numbers and file offsets.  There are 256 algorithm entries in this file and they are indexed by the algorithm index that is specified as the second operand to the “0x27” request-seed message in the communications script as found in each PCF (Program Control File) a.k.a. wormcode file or segment 0 file.

The 13 bytes of each algorithm entry has the following definition:

uu o1 v1h v1l o2 v2h v2l o3 v3h v3l o4 v4h v4l

where:	uu = Unknown “13th byte”
o1 = opcode 1
v1h = HH byte of operand value 1
v1l = LL byte of operand value 1
o2 = opcode 2
v2h = HH byte of operand value 2
v2l = LL byte of operand value 2
o3 = opcode 3
v3h = HH byte of operand value 3
v3l = LL byte of operand value 3
o4 = opcode 4
v4h = HH byte of operand value 4
v4l = LL byte of operand value 4

To illustrate the overall process, let’s pull one of the algorithm entries at random and calculate it:

Index: 24 Offset: 000001D4:  4A 2A 5B 5A 6B 0C 03 7E 90 8B 14 14 24

Here, our “13th byte” is 0x4A.  And we have 4 operations: 0x2A, 0x6B, 0x7E, and 0x14 as follows:

0x2A = Complement, and since 0x5B > 0x5A then it is a 2’s complement
0x6B = Rotate Right, since LL = 0x03 then ROR 3
0x7E = Swap and Add, since 0x90 >= 0x8B then add HHLL which is 0x908B
0x14 = Add HHLL which is 0x1424

So the full algorithm is 2’s complement, rotate-right 3, swap hi/low bytes and add 0x908B, and then add 0x1424.  Thus, given the seed 0x1234: a) ~0x1234 = 0xEDCB b) 0xEDCB ROR 3 = 0x7DB9 c) 0x7DB9 swab = 0xB97D then add 0x908B = 0x4A08 d) 0x4A08 + 0x1424 = 0x5E2C.  Thus the key corresponding to the seed of 0x1234 is 0x5E2C.

Note that currently all GM seed/key values are 16-bit unsigned numbers.  And the seed doesn’t “revolve” in the PCM – that is, each time it is requested from the same PCM the same value is returned.  There is also nothing in the PCM code itself that looks like a seed/key algorithm, so apparently the two values (seed and key) are stored as values somewhere in the internals of the PCM.  Though there is nothing to prevent GM on future PCM’s from making the seed a revolving value and actually calculate the corresponding key on the fly.

Conclusion
It appears that GM, currently, only plans to implement 256 different seed/key algorithms.  And, it appears that all 256 were devised and set in stone during the early years of PCM development.  Obviously they can’t go back and change algorithms, at least not those already in use, otherwise it would be impossible to correctly unlock vehicles in the field.  However, there is a possibility that they could change the algorithms that currently aren’t being used – and from observing the different algorithms that are being used, it appears that most are still not used.  Also, since these algorithms don’t change, it shouldn’t matter which CD this file is obtained from.  The file originally used for our observation was from CD 97-13, but it has been cross-checked with the one from CD 98-21 and the two were identical – as expected.

Thus we can conclude that currently we now have all GM seed/key algorithms both for past/current vehicles and for future vehicles.  And even if they were to change the algorithms to be used on future vehicles, we know the method of storage and interpretation of these algorithms, and so we can always derive the new algorithm.  However, if they were to change both the future algorithms and the method of storing and maintaining this table of algorithms, then we’ll have to go back to the code again to search for where/how they are stored.  But even then, we’ll still have the benefit of knowing existing exact algorithms from all past/current vehicles.
STGTERM.DAT
00 00000000 85 B6 96 0A 2A A9 21 41 4B 52 E7 2E 01 ....*.!AKR...
01 0000000D 01 14 61 02 6B 6A 05 7E CB 03 4C 06 6E ..a.kj.~..L.n
02 0000001A 6F 7E ED 96 14 E4 E0 6B CB 01 2A 61 87 o~.....k..*a.
03 00000027 87 14 D0 19 7E E2 7E 4C 09 FB 2A A6 F4 ....~.~L..*..
04 00000034 F5 98 B0 FC 7E DB 7F 4C 06 1A 14 DA 68 ....~..L....h
05 00000041 69 14 E2 C5 98 81 51 6B D5 02 2A 08 A0 i.....Qk..*..
06 0000004E A1 2A A9 3A 14 01 BF 6B ED 0B 4C 05 CD .*.:...k..L..
07 0000005B CD 14 AE 8D 7E FB D9 4C 02 F7 98 CA 34 ....~..L....4
08 00000068 35 2A C0 7B 14 6F 58 98 C1 26 4C 06 C0 5*.{.oX..&L..
09 00000075 C1 14 01 38 4C 05 12 7E 46 96 2A E2 02 ...8L..~F.*..
0A 00000082 03 14 42 C4 98 B6 BC 4C 05 2E 7E 30 2C ..B....L..~0,
0B 0000008F 3D 98 38 08 7E F2 94 6B E0 02 4C 03 48 =.8.~..k..L.H
0C 0000009C C9 7E 72 E0 98 B9 B0 14 5C 27 2A AB 08 .~r.....\'*..
0D 000000A9 01 7E C0 3A 2A A4 0D 6B 47 05 4C 0B 05 .~.:*..kG.L..
0E 000000B6 85 6B 50 02 7E 50 D2 4C 05 FD 98 18 CB .kP.~P.L.....
0F 000000C3 3B 7E 30 CF 6B 04 05 2A D5 2E 98 D9 DA ;~0.k..*.....
10 000000D0 DB 7E A0 0F 14 6B 04 6B 00 05 4C 01 85 .~...k.k..L..
11 000000DD 85 98 32 69 14 FC B3 7E F9 CB 6B AE 03 ..2i...~..k..
12 000000EA 03 7E B0 63 4C 01 40 14 4E 42 98 CA 34 .~.cL.@.NB..4
13 000000F7 35 98 2C 01 14 C0 28 2A 43 69 6B C6 01 5.,...(*Cik..
14 00000104 01 4C 01 89 2A D3 D6 14 8A 29 7E E0 47 .L..*....)~.G
15 00000111 45 4C 01 14 2A DE CC 98 97 75 14 8A 1B EL..*....u...
16 0000011E 55 2A 52 96 7E 60 AC 14 F0 56 6B 47 01 U*R.~`...VkG.
17 0000012B 01 7E 8C 08 4C 09 B0 14 F4 AB 2A 62 CB .~..L.....*b.
18 00000138 CB 14 81 4A 4C 06 E4 98 91 70 2A 05 4C ...JL....p*.L
19 00000145 4D 14 18 46 4C 06 60 98 70 73 7E 18 90 M..FL.`.ps~..
1A 00000152 91 4C 07 0D 98 B0 83 14 30 85 2A C2 18 .L......0.*..
1B 0000015F 19 6B 14 03 2A 34 C0 14 34 D1 4C 01 D1 .k..*4..4.L..
1C 0000016C D1 7E 80 3F 14 41 81 4C 0B 93 98 B9 73 .~.?.A.L....s
1D 00000179 73 4C 05 21 14 1E 17 7E 77 CB 2A CD 42 sL.!...~w.*.B
1E 00000186 43 98 07 4F 14 18 01 7E 0D 3B 2A 02 FE C..O...~.;*..
1F 00000193 FF 7E D3 5B 14 52 2C 4C 01 0C 98 4A 6E .~.[.R,L...Jn
20 000001A0 6F 98 90 48 6B 04 07 14 10 8A 7E 04 98 o..Hk.....~..
21 000001AD 98 14 0C 5C 4C 01 3E 6B 2B 09 98 86 30 ...\L.>k+...0
22 000001BA 30 6B A5 05 2A 0E D4 7E 46 69 14 E0 1F 0k..*..~Fi...
23 000001C7 1E 4C 05 64 14 10 94 2A A2 04 98 B1 4B .L.d...*....K
24 000001D4 4A 2A 5B 5A 6B 0C 03 7E 90 8B 14 14 24 J*[Zk..~....$
25 000001E1 24 2A 96 31 98 01 01 14 80 38 6B 0E 02 $*.1.....8k..
26 000001EE 02 14 2A 02 6B D5 01 7E F8 01 2A 25 07 ..*.k..~..*%.
27 000001FB 06 98 1F DE 7E E1 B7 14 8E 19 4C 09 24 ....~.....L.$
28 00000208 24 14 52 01 7E 38 97 2A BE 38 98 D4 28 $.R.~8.*.8..(
29 00000215 28 14 D1 80 4C 0B 00 98 F2 CC 2A 51 0C (...L.....*Q.
2A 00000222 0C 4C 0A 28 14 D3 C5 7E F8 63 6B FF 0B .L.(...~.ck..
2B 0000022F 0A 4C 07 00 7E EE 18 14 D0 C1 2A 0A 3A .L..~.....*.:
2C 0000023C 3A 98 90 39 6B 20 07 4C 0B 2A 14 0F 04 :..9k .L.*...
2D 00000249 04 2A B2 30 14 30 AA 7E 74 F7 6B 40 07 .*.0.0.~t.k@.
2E 00000256 06 14 1F 23 6B 3C 0B 2A 50 F5 7E F4 1F ...#k<.*P.~..
2F 00000263 1E 7E F4 1F 2A 0C 4C 98 01 AC 4C 06 96 .~..*.L...L..
30 00000270 96 4C 0A 80 14 0F 56 98 97 2F 7E 64 27 .L....V../~d'
31 0000027D 26 7E FB 95 4C 0A F1 2A 63 75 98 64 1E &~..L..*cu.d.
32 0000028A 1E 14 51 A2 2A 94 7B 6B 08 0B 4C 02 40 ..Q.*.{k..L.@
33 00000297 40 2A C0 C0 98 77 28 4C 01 0D 14 41 01 @*...w(L...A.
34 000002A4 00 7E A8 22 6B 80 01 98 17 BD 14 8E 20 .~."k....... 
35 000002B1 20 14 F0 86 6B B9 06 98 B1 09 4C 02 C1  ...k.....L..
36 000002BE C0 2A 21 80 6B 98 06 14 20 17 4C 02 1C .*!.k... .L..
37 000002CB 1C 6B 6F 03 4C 09 11 14 2A 0D 98 2C 91 .ko.L...*..,.
38 000002D8 90 4C 01 31 98 3A 72 2A 9C 3D 7E 99 ED .L.1.:r*.=~..
39 000002E5 EC 6B 65 07 4C 0A 77 7E F8 DA 98 3F 52 .ke.L.w~...?R
3A 000002F2 52 2A 1E 79 6B 99 03 7E 5B A2 98 73 1E R*.yk..~[..s.
3B 000002FF 1E 14 31 84 7E 6E E0 98 88 87 2A 6D 41 ..1.~n....*mA
3C 0000030C 40 2A 0C E7 6B 5A 07 7E B2 98 14 F1 51 @*..kZ.~....Q
3D 00000319 50 2A 54 A6 14 59 D5 4C 06 72 6B 57 0A P*T..Y.L.rkW.
3E 00000326 0A 2A 5F 46 6B 41 03 98 05 64 7E B3 D4 .*_FkA...d~..
3F 00000333 D4 2A 8B 90 4C 05 61 7E 94 80 14 5D 26 .*..L.a~...]&
40 00000340 26 98 6A 65 4C 07 B2 2A BC 72 7E 76 71 &.jeL..*.r~vq
41 0000034D 70 98 7F E6 2A 2E B1 4C 06 70 14 06 67 p...*..L.p..g
42 0000035A 66 7E F1 3E 4C 01 05 2A 10 60 14 93 C1 f~.>L..*.`...
43 00000367 C0 7E E4 35 6B 78 06 2A 10 16 98 A7 7B .~.5kx.*....{
44 00000374 7A 4C 02 EB 98 3B E7 14 43 30 6B AB 06 zL...;..C0k..
45 00000381 06 2A 84 B5 98 01 26 14 BB 65 7E 41 04 .*....&..e~A.
46 0000038E 04 7E CC 06 14 CD F6 6B 4A 07 4C 05 8F .~.....kJ.L..
47 0000039B 8E 14 6A CA 7E 6A 1C 2A A9 31 6B AC 07 ..j.~j.*.1k..
48 000003A8 06 98 BC C7 14 CB B3 7E C5 E7 4C 07 DC .......~..L..
49 000003B5 DC 7E E5 9D 6B 4B 06 4C 02 8A 98 03 AF .~..kK.L.....
4A 000003C2 AE 98 5F AD 4C 07 05 6B 22 02 14 39 8A .._.L..k"..9.
4B 000003CF 8A 2A 49 31 98 E0 F2 6B CB 0A 4C 0B EA .*I1...k..L..
4C 000003DC EA 2A 7E 5A 98 08 62 4C 05 0C 6B E5 05 .*~Z..bL..k..
4D 000003E9 04 7E FA D7 14 08 50 4C 0A 6E 98 C6 2D .~....PL.n..-
4E 000003F6 2C 7E FF 12 14 AD D3 98 E0 35 2A 97 27 ,~.......5*.'
4F 00000403 26 4C 0A 30 14 08 50 7E 35 F8 6B 25 02 &L.0..P~5.k%.
50 00000410 02 6B D2 06 2A 4C A5 4C 03 4C 7E A2 8E .k..*L.L.L~..
51 0000041D 8E 14 1E B8 7E E1 A2 6B 84 0B 98 60 10 ....~..k...`.
52 0000042A 10 6B 83 03 14 60 BC 7E 20 9C 98 02 18 .k...`.~ ....
53 00000437 18 2A 1A EF 6B 1A 02 7E 99 F8 4C 01 0C .*..k..~..L..
54 00000444 0C 14 14 30 4C 0A 07 6B 01 01 7E E8 73 ...0L..k..~.s
55 00000451 72 14 10 83 4C 07 3B 6B EB 03 98 1E 01 r...L.;k.....
56 0000045E 00 14 B4 10 2A 3C 2D 7E B0 85 6B AC 03 ....*<-~..k..
57 0000046B 02 6B B8 05 14 A1 96 98 23 1D 7E C0 06 .k......#.~..
58 00000478 06 4C 06 37 6B 84 07 98 53 CB 7E 2C 19 .L.7k...S.~,.
59 00000485 18 7E 50 D8 14 06 DD 98 0E EA 4C 03 D7 .~P.......L..
5A 00000492 D6 4C 02 23 14 70 F3 98 85 17 2A 9A E8 .L.#.p....*..
5B 0000049F E8 98 64 9A 6B 9A 01 14 41 40 7E 10 6E ..d.k...A@~.n
5C 000004AC 6E 4C 0A B4 2A DF DC 98 76 0A 7E 20 C1 nL..*...v.~ .
5D 000004B9 C0 4C 0A 71 6B 49 01 7E C0 47 14 F4 8F .L.qkI.~.G...
5E 000004C6 8E 98 6B ED 2A 87 A7 14 D0 F1 6B F9 03 ..k.*.....k..
5F 000004D3 02 14 75 07 4C 09 C1 6B 59 06 7E 08 73 ..u.L..kY.~.s
60 000004E0 72 7E 20 22 4C 01 0E 14 40 10 98 A9 11 r~ "L...@....
61 000004ED 10 4C 0A 86 14 4F 54 6B 50 01 2A 1B 06 .L...OTkP.*..
62 000004FA 06 2A 58 A5 14 59 51 6B D4 01 7E 10 40 .*X..YQk..~.@
63 00000507 40 4C 05 58 14 08 20 6B 02 0B 2A A0 92 @L.X.. k..*..
64 00000514 92 98 C2 24 14 9A E3 4C 07 08 6B 07 05 ...$...L..k..
65 00000521 04 6B 61 06 7E 75 CB 4C 01 0A 14 63 77 .ka.~u.L...cw
66 0000052E 76 98 64 47 4C 07 2F 2A 9E D0 7E 90 87 v.dGL./*..~..
67 0000053B 86 2A 39 D6 7E E2 87 98 E7 07 4C 05 29 .*9.~.....L.)
68 00000548 28 4C 05 6A 7E F8 10 98 8F 50 2A 40 56 (L.j~....P*@V
69 00000555 56 2A 0D C6 7E 07 B8 6B 1B 0B 14 B0 35 V*..~..k....5
6A 00000562 34 98 D7 B6 6B 01 0B 4C 03 FC 7E F7 BF 4...k..L..~..
6B 0000056F BE 7E 84 03 2A 30 16 98 67 C9 14 84 5E .~..*0..g...^
6C 0000057C 5E 4C 05 0E 14 FD 83 98 E4 0C 7E 8F BF ^L........~..
6D 00000589 BE 7E 10 E4 6B 5E 0B 4C 05 DF 98 84 39 .~..k^.L....9
6E 00000596 38 7E 0E 8F 4C 05 01 98 A4 6E 6B B3 06 8~..L....nk..
6F 000005A3 06 4C 0A 1C 98 B0 61 2A BA 02 14 76 40 .L....a*...v@
70 000005B0 40 4C 03 08 14 89 2E 7E 86 42 6B 28 02 @L.....~.Bk(.
71 000005BD 02 4C 06 2C 6B 2F 09 98 85 FB 2A 30 3E .L.,k/....*0>
72 000005CA 3E 14 B1 C2 98 BC 3D 6B 25 0B 2A E5 B8 >.....=k%.*..
73 000005D7 B8 2A D6 02 4C 01 4B 6B EC 03 98 5B 83 .*..L.Kk...[.
74 000005E4 82 6B C2 07 14 BE 2B 98 D6 CA 2A 77 92 .k....+...*w.
75 000005F1 92 98 60 FA 14 D2 30 4C 09 55 7E 0E 24 ..`...0L.U~.$
76 000005FE 24 4C 0B D0 14 E7 96 98 70 F9 6B 79 0B $L......p.ky.
77 0000060B 0A 98 6F 89 4C 0B 30 6B 40 07 14 20 2D ..o.L.0k@.. -
78 00000618 2C 6B 9A 09 2A AD DA 14 BE 8C 4C 0B CB ,k..*.....L..
79 00000625 CA 7E 0D D6 2A DC 51 14 E0 0C 6B 0E 03 .~..*.Q...k..
7A 00000632 02 4C 0B FF 2A D0 C6 6B 1E 03 7E 86 4A .L..*..k..~.J
7B 0000063F 4A 7E B7 9A 4C 05 B3 14 75 E0 6B C0 0B J~..L...u.k..
7C 0000064C 0A 14 87 78 4C 01 D0 98 04 6E 2A 28 07 ...xL....n*(.
7D 00000659 06 2A 98 06 6B 0A 0A 4C 0A 51 98 F4 EB .*..k..L.Q...
7E 00000666 EA 6B 0A 03 14 E3 1D 7E D8 A3 98 A0 48 .k.....~....H
7F 00000673 48 6B 2C 09 14 50 04 98 80 40 7E F9 33 Hk,..P...@~.3
80 00000680 32 98 61 DF 14 02 25 7E C4 4B 2A A0 66 2.a...%~.K*.f
81 0000068D 66 14 1D 47 7E 85 0E 2A 84 A1 98 BC E6 f..G~..*.....
82 0000069A E6 14 F4 37 98 0E 31 6B 74 06 2A 3B 8A ...7..1kt.*;.
83 000006A7 8A 14 57 CC 7E A3 0C 98 50 54 2A F2 AB ..W.~...PT*..
84 000006B4 AA 14 3D 06 7E F4 45 6B C4 01 4C 01 C3 ..=.~.Ek..L..
85 000006C1 C2 7E 05 52 2A EA 51 4C 03 EF 14 FF D8 .~.R*.QL.....
86 000006CE D8 7E 45 13 2A 54 60 98 52 4C 14 A0 F8 .~E.*T`.RL...
87 000006DB F8 98 0A D0 4C 09 03 6B 25 09 14 74 1D ....L..k%..t.
88 000006E8 1C 98 AE 06 14 B9 6B 7E B1 2D 6B 00 07 ......k~.-k..
89 000006F5 06 14 58 04 4C 05 20 7E A1 04 98 3A D4 ..X.L. ~...:.
8A 00000702 D4 98 8C 3D 14 70 8F 4C 06 10 7E 02 C0 ...=.p.L..~..
8B 0000070F C0 4C 02 F7 7E A5 63 2A 01 29 14 11 95 .L..~.c*.)...
8C 0000071C 94 14 C2 1A 98 C2 4C 6B CC 06 2A 31 C1 ......Lk..*1.
8D 00000729 C0 14 40 13 7E 74 87 98 95 4B 2A 4F 15 ..@.~t...K*O.
8E 00000736 14 14 21 DB 98 2F 03 2A AB 58 7E 48 DF ..!../.*.X~H.
8F 00000743 DE 14 14 F0 98 03 09 2A E2 D4 4C 02 C3 .......*..L..
90 00000750 C2 98 A0 43 14 80 0B 2A 2D 01 4C 02 57 ...C...*-.L.W
91 0000075D 56 6B 11 03 2A E2 0E 98 B7 54 14 92 E0 Vk..*....T...
92 0000076A E0 4C 06 58 98 A0 60 2A 10 40 14 EE 4C .L.X..`*.@..L
93 00000777 4C 98 2A 03 4C 05 2C 14 B0 13 2A D4 EB L.*.L.,...*..
94 00000784 EA 98 02 1D 14 5B 11 2A 77 80 7E D8 75 .....[.*w.~.u
95 00000791 74 6B 05 02 7E AB 16 14 21 93 4C 06 B8 tk..~...!.L..
96 0000079E B8 7E 80 0B 4C 05 59 2A 80 8E 14 1D A3 .~..L.Y*.....
97 000007AB A2 14 75 B8 4C 05 98 98 11 5D 7E D4 4F ..u.L....]~.O
98 000007B8 4E 4C 06 02 14 D8 41 6B EF 07 98 A6 1C NL....Ak.....
99 000007C5 1C 7E 5E BD 98 60 B7 6B 04 06 2A A4 43 .~^..`.k..*.C
9A 000007D2 42 2A 87 3F 7E 18 82 98 F6 B3 14 38 FC B*.?~......8.
9B 000007DF FC 4C 09 48 2A 71 FA 14 6C 91 98 F3 47 .L.H*q..l...G
9C 000007EC 46 7E FB F5 14 5A A8 98 32 D8 2A CD 30 F~...Z..2.*.0
9D 000007F9 30 14 20 EA 98 0F 1D 4C 07 02 2A A0 03 0. ....L..*..
9E 00000806 02 14 D0 A1 7E 64 C1 4C 01 0A 2A FE 71 ....~d.L..*.q
9F 00000813 70 4C 01 FE 98 33 1A 2A C0 D4 7E 41 80 pL...3.*..~A.
A0 00000820 80 7E FC C2 4C 0B 3A 98 8A 66 14 D1 09 .~..L.:..f...
A1 0000082D 08 2A F1 80 14 E8 04 7E D4 01 6B 79 0B .*.....~..ky.
A2 0000083A 0A 4C 0A 7F 6B 38 0B 14 E1 4E 98 40 11 .L..k8...N.@.
A3 00000847 10 4C 09 1E 7E 1C FA 14 3F 1A 98 11 76 .L..~...?...v
A4 00000854 76 2A A2 0A 14 16 80 98 80 48 4C 09 8A v*.......HL..
A5 00000861 8A 14 80 88 2A 70 59 4C 01 3D 6B 04 07 ....*pYL.=k..
A6 0000086E 06 4C 05 28 98 80 19 14 C9 AC 2A 91 C6 .L.(......*..
A7 0000087B C6 6B 60 02 7E CB 25 14 74 02 98 B6 01 .k`.~.%.t....
A8 00000888 00 14 C3 0E 4C 02 30 2A 4E 30 98 29 80 ....L.0*N0.).
A9 00000895 80 98 08 D4 14 29 06 2A F4 BA 4C 05 16 .....).*..L..
AA 000008A2 16 7E 28 80 14 63 05 2A D1 61 4C 02 00 .~(..c.*.aL..
AB 000008AF 00 98 A0 48 6B 3E 01 4C 02 D8 14 81 0C ...Hk>.L.....
AC 000008BC 0C 98 1E 09 14 08 86 7E AA 1D 4C 06 F0 .......~..L..
AD 000008C9 F0 6B B6 01 14 7D A9 98 50 61 2A D2 A6 .k...}..Pa*..
AE 000008D6 A6 2A 13 1A 98 63 14 4C 02 40 7E F8 CA .*...c.L.@~..
AF 000008E3 CA 14 89 30 98 AE 50 2A 8D 60 7E 86 F1 ...0..P*.`~..
B0 000008F0 F0 14 A1 E6 98 06 B0 4C 06 04 6B F4 02 .......L..k..
B1 000008FD 02 4C 01 C0 2A 81 C8 14 02 0F 98 91 88 .L..*........
B2 0000090A 88 2A 2C A5 98 87 0A 7E 50 70 4C 01 80 .*,....~PpL..
B3 00000917 80 98 C0 D0 2A 6E 15 4C 01 26 6B F6 01 ....*n.L.&k..
B4 00000924 00 7E 1D 01 98 62 01 4C 0A F0 14 19 D0 .~...b.L.....
B5 00000931 D0 4C 03 43 14 71 CA 98 3F 81 2A 01 BA .L.C.q..?.*..
B6 0000093E BA 98 72 E6 14 1E 30 4C 01 44 7E F4 B6 ..r...0L.D~..
B7 0000094B B6 14 C3 92 6B 11 06 7E 8C D1 4C 06 B2 ....k..~..L..
B8 00000958 B2 4C 01 F0 14 5D D2 7E A6 1B 6B 32 03 .L...].~..k2.
B9 00000965 02 4C 02 EF 14 41 41 98 5B 1B 2A 4C D1 .L...AA.[.*L.
BA 00000972 D0 7E 47 1D 6B 8A 06 98 63 19 14 63 8D .~G.k...c..c.
BB 0000097F 8C 14 F0 03 2A 38 75 6B 21 02 4C 09 A6 ....*8uk!.L..
BC 0000098C A6 14 04 9C 7E 30 4B 4C 01 1E 2A B0 1E ....~0KL..*..
BD 00000999 1E 14 7D F6 7E 30 64 4C 02 9E 98 B2 05 ..}.~0dL.....
BE 000009A6 04 98 F4 80 4C 09 FD 2A 53 0C 7E A0 64 ....L..*S.~.d
BF 000009B3 64 6B 96 02 7E 58 DA 14 63 80 4C 01 72 dk..~X..c.L.r
C0 000009C0 72 4C 02 EC 7E 52 7D 98 DE FA 14 11 20 rL..~R}..... 
C1 000009CD 20 14 0C 98 6B 1A 0B 2A 80 D0 7E DA D2  ...k..*..~..
C2 000009DA D2 98 C2 15 14 0B D0 7E 32 0F 6B D9 03 .......~2.k..
C3 000009E7 02 98 E8 D7 14 13 C4 4C 09 50 7E 90 A2 .......L.P~..
C4 000009F4 A2 7E 9F 30 14 C5 1A 2A 6A 0B 4C 03 D8 .~.0...*j.L..
C5 00000A01 D8 14 91 69 2A 9D 11 98 BA F3 4C 07 3E ...i*.....L.>
C6 00000A0E 3E 2A 01 E0 7E C4 13 4C 0B 16 98 83 83 >*..~..L.....
C7 00000A1B 82 2A F4 50 7E 30 9B 4C 0B D8 14 BF 36 .*.P~0.L....6
C8 00000A28 36 7E 6E B5 14 CD BB 98 BF F0 4C 0A 3D 6~n.......L.=
C9 00000A35 3C 7E 02 79 4C 0B 9B 98 2D 5E 14 A1 03 <~.yL...-^...
CA 00000A42 02 14 35 FE 98 3D F0 7E 51 81 6B 55 02 ..5..=.~Q.kU.
CB 00000A4F 02 14 84 55 4C 02 36 6B 89 03 98 0C 60 ...UL.6k....`
CC 00000A5C 60 7E 78 F7 4C 01 90 98 C4 A7 6B 09 02 `~x.L.....k..
CD 00000A69 02 98 13 B3 7E 38 34 4C 05 68 14 13 C0 ....~84L.h...
CE 00000A76 C0 4C 07 00 7E 20 2C 2A 0C D2 6B 8A 0A .L..~ ,*..k..
CF 00000A83 0A 14 C0 3C 2A 0C 75 7E B7 3C 4C 09 FD ...<*.u~.<L..
D0 00000A90 FC 98 03 60 6B 1C 01 7E F0 01 4C 03 1C ...`k..~..L..
D1 00000A9D 1C 2A 31 0C 6B C1 02 14 01 BE 98 0E AD .*1.k........
D2 00000AAA AC 7E 40 0C 6B 07 06 2A 0D A5 98 8C F0 .~@.k..*.....
D3 00000AB7 F0 98 F3 ED 6B A8 07 14 0E A0 2A 81 94 ....k.....*..
D4 00000AC4 94 2A 0D D0 7E 48 0E 14 C0 01 98 16 44 .*..~H......D
D5 00000AD1 44 2A 1D C0 6B 65 09 14 EA C1 7E DE D0 D*..ke....~..
D6 00000ADE D0 7E 30 B9 14 F3 9C 6B 3E 03 4C 09 C0 .~0....k>.L..
D7 00000AEB C0 98 D1 F4 4C 01 A8 6B 15 02 2A 07 E1 ....L..k..*..
D8 00000AF8 E0 2A F7 14 98 60 7B 4C 06 F8 7E 46 FA .*...`{L..~F.
D9 00000B05 FA 2A 50 43 14 E8 1E 4C 07 D6 7E 12 4E .*PC...L..~.N
DA 00000B12 4E 14 38 C7 2A 61 13 6B FD 03 4C 06 DC N.8.*a.k..L..
DB 00000B1F DC 14 10 D0 98 C2 06 2A F1 01 6B D1 05 .......*..k..
DC 00000B2C 04 14 91 35 7E 17 21 2A 01 D0 4C 0A F0 ...5~.!*..L..
DD 00000B39 F0 6B 71 01 7E 9F BA 2A BF 78 4C 05 60 .kq.~..*.xL.`
DE 00000B46 60 4C 02 50 6B 2F 0B 2A A1 02 98 7D 26 `L.Pk/.*...}&
DF 00000B53 26 98 A5 D6 14 D1 80 6B 6B 0B 2A AE 36 &......kk.*.6
E0 00000B60 36 98 21 41 7E 02 38 14 C1 80 4C 01 1E 6.!A~.8...L..
E1 00000B6D 1E 7E 80 ED 14 D1 98 6B 2C 06 2A 0E 0C .~.....k,.*..
E2 00000B7A 0C 14 80 23 6B 4E 0B 2A 51 02 98 CE BA ...#kN.*Q....
E3 00000B87 BA 2A 48 01 6B 18 06 98 C6 1B 4C 07 BE .*H.k.....L..
E4 00000B94 BE 4C 0B 41 6B D9 03 14 80 1F 2A 31 3E .L.Ak.....*1>
E5 00000BA1 3E 4C 09 C1 7E 39 AF 98 3D 18 14 06 64 >L..~9..=...d
E6 00000BAE 64 98 BF D5 14 58 33 4C 02 46 7E 8C 78 d....X3L.F~.x
E7 00000BBB 78 14 FE 1E 7E 15 7E 98 52 80 4C 09 95 x...~.~.R.L..
E8 00000BC8 94 4C 01 68 7E 92 2B 14 4E 41 98 3B 14 .L.h~.+.NA.;.
E9 00000BD5 14 7E A0 22 98 19 1F 14 BC C8 4C 02 10 .~."......L..
EA 00000BE2 10 6B 94 05 4C 06 9F 98 D4 88 7E 91 5B .k..L.....~.[
EB 00000BEF 5A 98 1C 6B 2A B2 84 14 ED 59 4C 05 70 Z..k*....YL.p
EC 00000BFC 70 14 06 F0 7E 38 B0 2A 4F BE 4C 0B F0 p...~8.*O.L..
ED 00000C09 F0 14 D0 7D 2A 63 B4 6B 71 03 7E F1 E9 ...}*c.kq.~..
EE 00000C16 E8 2A 38 20 7E 8A 07 98 63 72 14 01 FC .*8 ~...cr...
EF 00000C23 FC 6B D4 03 4C 02 14 14 07 20 2A ED B1 .k..L.... *..
F0 00000C30 B0 2A 2A E9 14 0A A0 4C 01 58 6B 90 06 .**....L.Xk..
F1 00000C3D 06 4C 01 A5 7E 3D 6B 14 34 9E 98 40 01 .L..~=k.4..@.
F2 00000C4A 00 2A B0 C9 14 2B 80 98 13 CD 7E F8 4E .*...+....~.N
F3 00000C57 4E 2A 02 F9 14 92 FF 4C 01 0F 98 20 5E N*.....L... ^
F4 00000C64 5E 14 1C F0 6B C9 01 98 35 91 4C 01 BD ^...k...5.L..
F5 00000C71 BC 4C 06 1D 14 D0 C2 7E 0D 81 6B 66 09 .L.....~..kf.
F6 00000C7E 08 14 BC 17 4C 03 40 98 7B F1 2A 0D 5A ....L.@.{.*.Z
F7 00000C8B 5A 98 02 8C 7E 86 AC 4C 03 1B 2A 21 38 Z...~..L..*!8
F8 00000C98 38 14 C8 16 4C 01 76 2A 2C 7C 98 10 F0 8...L.v*,|...
F9 00000CA5 F0 6B 28 06 4C 01 18 98 E8 63 14 9C 18 .k(.L....c...
FA 00000CB2 18 14 E0 B9 2A BF 0C 98 02 30 4C 01 8F ....*....0L..
FB 00000CBF 8E 14 04 82 2A 15 38 4C 0A C1 98 64 C3 ....*.8L...d.
FC 00000CCC C2 6B 0E 07 14 28 61 4C 01 02 2A 7F D2 .k...(aL..*..
FD 00000CD9 D2 14 15 34 2A AE F2 7E 02 04 4C 01 38 ...4*..~..L.8
FE 00000CE6 38 98 2A 86 6B E7 01 14 2F 98 2A 06 89 8.*.k.../.*..
FF 00000CF3 88 6B 01 03 98 B2 26 14 28 DE 2A 9B 78 .k....&.(.*.x


# EpicFilePatcher

### Instructions:
   1. Multiselect the file you want to match and the patch file (.efptxt)
   2. Drag them onto EpicFilePatcher
   3. Profit

## Documentation
- Notes:
  - All the instructions are case **in**sensitive, though its preferred for them to be capitalizd.
  - Supports semicolons
  - 'int' generally will mean an 8 byte long instead of a 4 byte int here

### Instruction Set
<!--
<details><summary>
<b>Instruction Set</b>
</summary>
-->
- `GOTO`
  - Description:
    - Sets writing positing to the int given
  - Syntax: GOTO [int]
  - Affected by endian: `false`
  - Examples:
    - `GOTO 0xAE30C0`
    - `GOTO 1000`
- `WRITE`
  - Description:
    - Writes bytes given sequentially
    - Endian affects each byte individually
  - Syntax: WRITE [bytes]
  - Affected by endian: `true`
  - Examples:
    - `WRITE 3f 7f`
    - `WRITE 1f4fCCCC72`
- `INT16`, `INT32`, `INT64`
  - Description:
    - Writes the int given
  - Syntax: INT[16/32/64] [int]
  - Affected by endian: `true`
  - Examples:
    - `INT64 0x143FFBA28`
    - `INT16 32767`
- `WRITESTRING`
  - Description:
    - Writes string given
    - Does not write a null after the string
  - Syntax: WRITESTRING [string]
  - Affected by endian: `false`
  - Examples:
    - `WRITESTRING "HelloWorld!"`
- `WRITESTRINGN`
  - Description:
    - Writes string given, and keeps writing nulls until it finds a null
    - Useful for if you are replacing a string in something that is bigger than your given string
  - Syntax: WRITESTRINGN [string]
  - Affected by endian: `false`
  - Examples:
    - `WRITESTRINGN "HelloWorld!"`

### Options Instruction Set
<!--
<details><summary>
<b>Instruction Set</b>
</summary>
-->
Note: All options instructions are prefixed with @
- `INCLUDE`
  - Description:
    - Includes the code from another file where this instruction is written
  - Syntax: INCLUDE [path: string]
  - Examples:
    - `@INCLUDE "offsets.efptxt"`
- `OFFSET`
  - Description:
    - Offsets the GOTO instruction
    - Set position = (GOTO instruction) + offset
  - Syntax: OFFSET [int]
  - Examples:
    - `@OFFSET 0x3F000`
- `LITTLEENDIAN` / `LITTLE` / `SWITCHLITTLEENDIAN`
  - Description:
    - Sets the current endian that affects the writing of some instructions
  - Syntax: [LITTLEENDIAN/LITTLE/SWITCHLITTLEENDIAN]
  - Examples:
    - `@LITTLEENDIAN`
- `BIGENDIAN` / `BIG` / `SWITCHBIGENDIAN`
  - Description:
    - Sets the current endian that affects the writing of some instructions
  - Syntax: [BIGENDIAN/BIG/SWITCHBIGENDIAN]
  - Examples:
    - `@BIGENDIAN`


</details>
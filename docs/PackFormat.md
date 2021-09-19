# Pack - File Information/Structure

*// *These notes are built off [Rhett's technical breakdown](https://github.com/RhettVX/forgelight-toolbox/blob/master/docs/rhett-pack1-notes.txt).*

**Extension**: `.pack`\
**Endianness**: Big\

### Description

Pack files were the method of storing asset data for games that run on the ForgeLight engine. This format has since been replaced by [Pack2](pack2.md).

### Format

Pack files are comprised of chunks. Each chunk has an 8 byte header and then a block of asset headers describing the stored assets. The chunks are *not* terminated, so you will have to utilise the offset with the chunk header to know when you have reached the next chunk.

Continue reading data until the chunk header equals `0x00`. At this point there is a buffer of `0x00` before the asset data begins.

#### Chunk Header

Name       | Type   |   Example   | Description
---------- | ------ | ----------- | ---
Next Chunk | uint_32 | 00 83 7e d1 | The offset of the next chunk in the pack
Asset Count | uint_32 | 00 00 00 75 | The number of assets in the chunk

#### Asset Header

Name          | Type   |   Example   | Description
------------- | ------ | ----------- | ---
Name Length   | `uint_32` | `00 00 00 1a` | The length of the asset name
Asset Name    | `char_8[]` | `AMB_HOSSIN_NIGHT_OS_16.fsb` | The name of the asset file. Not `NUL` terminated.
Asset Offset  | `uint_32` | `00 05 aa 3c` | The offset within the pack of the asset data
Data Length   | `uint_32` | `00 00 57 80` | The length of the asset data
Checksum      | `uint_32` | `31 4c 45 61` | A CRC32 checksum of the asset data

#### Overall read process

1. Read a chunk header.
2. Read its asset headers till the next chunk offset.
3. Repeat steps 1-2 until the `0x00` block is reached.
4. Read the asset data.
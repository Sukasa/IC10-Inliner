# IC10 Inliner

This is a small code project that helps to organize and minify ic10 programs.  It accepts standard ic10 assembly, parses it, and spits out minified/inlined assembly.  This project is primarily meant for larger or multi-party projects, to allow you to reap the benefits of extensively-documented code, while not giving up any space efficiency in the in-practice asssembly.

## Features 
The full set of features is:
- Minifies assembly, by taking `define` and `alias` lines and removing them, inlining the values into your assembly
- Notwithstanding the above, keeps `alias` lines that refer to device pins directly (d0, d1, etc)
- Removes all labels, entering the fixed line offsets for them in place
- Supports a unique `section` directive that allows you to split an ic10 program into parts, and selectively assemble these sections into the output via command line
- Does basic parameter checks on assembly files

## Development

Eventually I want to port this to a webpage for ease of use, but for now you'll need to either grab a release or compile it yourself

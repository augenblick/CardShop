Just a quick place for me to jot design thoughts down


Card, Box, Package are all products
"OpenProduct" updates player inventory and returns the created contents.
Products contain a "Openable" bool.  If one attempts to open a Product where Openeable == false, an error is returned.

Questions -----------------
Should product "contents" definition be 'redacted' from response?
Should multiples of the same product be enumerated, or simply include a "count" property?




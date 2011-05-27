Use RESX Generator to translate from one language to another.

Create a TXT resource file (either by converting RESX to txt using resgen or directly creating it)
eg: resgen ResourceLibrary.resx german.txt
http://msdn.microsoft.com/en-us/library/ccec7sz1(v=vs.80).aspx

USING FSI
---------

1) Run the ResxGenerator.fsx using fsi or right click and run.
2) Enter the inputs as asked by the tool.
3) This will create a new file appended with "to language" text.
4) Convert the text file back to resx with the same extension.
Note: It uses BING translator, so be sure to have the internet connected.

USING EXE
---------

1) Run the Resx helper as,
resxhelper.exe /f:C:\somelocation\someresx.txt /from:en /to:de /o:output.de.txt

2) Once when the translation is done, convert the output file back to RESX using resgen
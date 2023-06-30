# PT2023

To run it, first you need to build it as an x64 application. 

If you want to use it for a language different than English, you need to go to the SpeechToText class.
alsmost at the beginning of the file, there is a variable: 
private string currentLanguage="en-GB"; 
you will need to change this value, based on your windows installation
for example: if you wat to use it for Greece, the variable should be: 

private string currentLanguage="el-GR"; 
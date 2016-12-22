# Space Engineers Ingame Scripting Base Solution

This repo is a visual studio solution to support proper intellisense in visual studio while writing code for ingame scripts. It is set up in a way that lets you create the script as if it is and actual program (making development much easier). It is split up into 2 projects, the build project and scripting project.

## Using the Solution

### Scripting Project

The scripting project is the project you should use in order to write any of your scripts. Before anything, you should check that the references to the files **Sandbox.Common.dll**, **Sandbox.Game.dll**, **VRage.Game.dll** and **VRage.Math.dll** are all correct in the `Solution -> Scripts -> References` item in the solution explorer (all files are located the Bin64 folder in your Space Engineers install folder, which defaults to (on x64 systems) **C:\Program Files (x86)\Steam\SteamApps\common\SpaceEngineers\Bin64**). Once you have made sure that the references are in place, you can write a script with full intellisense.

To start, I recommend you copy the file **BaseProgram.cs** and rename it, since it allows you to keep this default file for future scripts (if you do this, then make sure to rename the namespace `BaseProgram` to the name of your script, as this stops the classes from interfering with each other). This file is set up so that you can start writing your script immediately.

After you have finished writing your script, you will have to make sure to edit it in order to make it compatible with the Space Engineers programmable block. The code that must be removed is already conclosed with
```cs
/*-*/ ... /*-*/
```
blocks, and removing all the code beween the blocks (there are 2 by default, one a the beginning and one at the end) will make the script compatible. However, this would be time-consuming to do each time, so I have written a build program to automatically do this for you.

### Building Project

Because the base script contains code at the start and end that needs to be removed each time you copy it into Space Engineers, I have written a program that will automatically remove the code contained inside the blocks for you and output it into a file. However, in order to make the output nice, it can also do a variety of other things. In order to use this program, you have to compile it. It should compile on most .NET Framework versions, however I have only tested on v4.5.2.
The behaviour of this program is fully dictated by the **settings.xml** file that it will generate automatically on the first run (meaning that you shouldn't have to edit the code at all), and is the best way of explaining the functions of the program.

#### Setting: `path_file_in`

This is the path the directory that contains the scripts you actually want to run through the builder. Unless you are putting your files inside other directories, you shouldn't need to modify this.  
Default (this is because this is the location of the **BaseProgram.cs**):

```xml
<path_files_in>..\..\..\Scripts</path_files_in>
```

#### Setting: `path_file_out`

This is the path to the output directory that the built scripts will be written to. As long as the parent directory exists, it will be fine since it will automatically generate the actual directory for you.  
Default:

```xml
<path_files_out>..\..\..\Scripts\out</path_files_out>
```

#### Setting: `ignore_files`

This is a list of xml elements with the tag `<file>` which contain the values of the files you dont want to include in the build process. It is the name of the file itself, not the path to it (meaning it only affects the files found in the `path_file_in` setting).  
Default (this is because this is a template file):

```xml
<ignore_files>
    <file>BaseProgram.cs</file>
</ignore_files>
```

#### Setting: `remove_multiline_comments`

This is a boolean value stating whether the builder should remove multiline comments.  
Default:

```xml
<remove_multiline_comments>true</remove_multiline_comments>
```

#### Setting: `remove_newlines`

This is a boolean value stating whether to remove all newlines that are outside strings (to make sure it doesn't mess with actual code). This is incase you want to compact your code as much as possible in order to fit the character limit, however for most programs it shouldn't be necessary.  
Default:

```xml
<remove_newlines>false</remove_newlines>
```

#### Setting: `remove_singleline_comments`

This is a boolean value stating whether to remove all single line comments.  
Default:

```xml
<remove_singleline_comments>true</remove_singleline_comments>
```

#### Setting: `remove_whitespace`

This is a boolean value stating whether to remove all whitespace in code. This essentially means any text that is 2 or more spaces (doesn't include newlines). It will ignore spaces inside string to preserve your code.
Default:

```xml
<remove_whitespace>true</remove_whitespace>
```

#### Builder notes

If the settings file is missing or corrupt, it will just rewrite it and use the default settings. The output files made by this program should be able to be copied straight into your programmable block. The reason you can choose to remove newlines outside strings is because C# doesn't actually need newlines to seperate lines, that is what the curly brackets and semicolon is for. Thanks to this, you can have scripts that are increadibly compact in order to save space.

### Acknowledgements and Side Notes

This is possible to [CyberVic](http://forum.keenswh.com/members/cybervic.3115311/) on the KeenSWH forums for coming up with a guide to setting the base file up. I doubt I would have done this without [this](http://forum.keenswh.com/threads/guide-setting-up-visual-studio-for-programmable-block-scripting.7225319/) post.

The template file is fully commented in order to enhance your understanding of the ingame programming (writing it certainly did for me). The tips included in the file I will hopefully update in the future as I discover more bits and as the game changes. Also, the basic layout of the `Program`, `Save`, and `Main` functions are taken from the default code the programmable block gives you.

I haven't tested the build program on large scripts, I believe that it should be fine but take a long time because the regex will take a while to process (if you leave it, it should be fine). Finally, the builder program is actually based on a script I wrote (which I decided to immediately convert to C#), which you can find [here](https://gist.github.com/Gorea235/26c85a150d7f94c768960fcda9734014) (if you're interested).
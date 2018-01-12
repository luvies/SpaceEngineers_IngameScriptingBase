# Space Engineers Ingame Scripting Base Solution

This project is a solution to write and build ingame scripts for Space Engineers fluently. The base program file allows for proper intellisense support, and the builder project allows conversion to SE-format and can minimise the code (to get round programmable block limits).

## Using the Solution

### Scripting Project

The scripting project is the project you should use in order to write any of your scripts. Before anything, you should check that the references to the game libraries are all correct in the `Solution -> Scripts -> References` item in the solution explorer (all files are located the Bin64 folder in your Space Engineers install folder, which defaults to (on x64 systems) **C:\Program Files (x86)\Steam\SteamApps\common\SpaceEngineers\Bin64**). Most, if not all, of the libraries that you will need to reference are specified in the base script. Once you have made sure that the references are in place, you can write a script with full intellisense.

To start, I recommend you copy the file **BaseProgram.cs** and rename it, since it allows you to keep this default file for future scripts (if you do this, then make sure to rename the namespace `BaseProgram` to the name of your script, as this stops the class names from interfering with each other). This file is set up so that you can start writing your script immediately.

To make the script SE-compatible, you will either need to extract the code from the script and copy it in, or (and this is much more recommened), run it through the builder also provided.

### Building Project

This is the project that houses the building program, and should be run when building all the scripts. Details on how it works are below.

## Building

Since scripts written using this method will not be directly compatible with Programmable Blocks, I have written a builder that can strip out the unnecessary parts of the program and make it usable. The main functionality is the custom blocks that process entire section of code directly (discussed below), but it can also strip out other unneeded things like whitespace, newlines, single line and multiline comments.

### Custom blocks & Minimisation

This is the main function of the builder. There are 2 custom blocks that are processed at the moment, the stripper block and the minimisation block. Each are used via tags derived from multiline comments, however these tags must be used *exactly* otherwise they are ignored as normal multiline comments.

#### Stripper Block

This block strips out all text between it, nothing more, nothing less. It's main purpose is to strip out the beginning and end sections that set up the usings and namespace/class declarations (and also the interface property inplementations).

This block is opened *and* closed using the following tag:

```csharp
/*-*/
```

#### Minifier Block

This block will minify the code between it. If there is code before or after it that is used by the minified block, then those references are kept (allows properly named config variables). This minification is done via the [CSharp-Minifier](https://github.com/KvanTTT/CSharp-Minifier) project, and is susceptible to its limitations. Currently, these are:

* Expression bodies (e.g. `void func() => 1;`) are not supported
    * The current workaround is to just use standard block bodies

This block is opened *and* closed using the following tag:

```csharp
/*m*/
```

### Settings File

The builder has a config file that allows you to modify execution. It is generated on first run, and by default, it is set up build all scripts in the 'Scripts' project into the 'Scripts/out' directory, and will preserve comments and newlines, while stripping whitespace. This means that any script built like this will immediately work in a Programmable Block, and is editable in it. If you have a lot of code, I recommend using the minimiser block rather than stripping comments and newlines (just so people can see the config vars at the top).

#### Setting: `path_file_in`

This is the path the directory that contains the scripts you actually want to run through the builder. Unless you are putting your files inside other directories, you shouldn't need to modify this.
Default (points to the directory containing the scripts in the 'Scripts' project):

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
<remove_multiline_comments>false</remove_multiline_comments>
```

#### Setting: `remove_newlines`

This is a boolean value stating whether to remove all newlines that are outside strings (to make sure it doesn't mess with actual code). This is incase you want to compact your code as much as possible in order to fit the character limit, however for most programs it shouldn't be necessary. (If this is true but `remove_singleline_comments` is false, then the newlines at the ends of the comments are preserved)

Default:

```xml
<remove_newlines>false</remove_newlines>
```

#### Setting: `remove_singleline_comments`

This is a boolean value stating whether to remove all single line comments.

Default:

```xml
<remove_singleline_comments>false</remove_singleline_comments>
```

#### Setting: `remove_whitespace`

This is a boolean value stating whether to remove all whitespace in code. This essentially means any text that is 2 or more spaces (doesn't include newlines). It will ignore spaces inside string to preserve your code.

Default:

```xml
<remove_whitespace>true</remove_whitespace>
```

#### Builder notes

If the settings file is missing or corrupt, it will just rewrite it and use the default settings. Unless you need to (e.g. different script in/out locations), I recommend leaving the config alone, since I've set up the config for the best built files.

### Acknowledgements and Side Notes

This is possible to [CyberVic](http://forum.keenswh.com/members/cybervic.3115311/) on the KeenSWH forums for coming up with a guide to setting the base file up. I doubt I would have done this without [this](http://forum.keenswh.com/threads/guide-setting-up-visual-studio-for-programmable-block-scripting.7225319/) post.

The template file is fully commented in order to enhance your understanding of the ingame programming (writing it certainly did for me). The tips included in the file I will hopefully update in the future as I discover more bits and as the game changes. Also, the basic layout of the `Program`, `Save`, and `Main` functions are taken from the default code the programmable block gives you.

Also huge thanks to the [CSharp-Minifier](https://github.com/KvanTTT/CSharp-Minifier) project, as writing this myself would be such a pain.
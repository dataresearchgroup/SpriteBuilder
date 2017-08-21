# SpriteBuilder
MSBuild task for sprite image and css compilation

## Usage
    <UsingTask AssemblyFile="$(ProjectDir)\..\lib\Drg.SpriteBuilder.dll" TaskName="BuildSprites" />
    <Target BeforeTargets="BeforeBuild">
      <BuildSprites ContentPath="$(ProjectDir)\Content" />
    </Target>
    
Currently this task only supports one argument: `ContentPath`. The ContentPath is the root content folder. Until other parameters are added, images to sprite must be located at $(ContentPath)\images\icons\ within folders dedicated to single size images. E.g. $(ContentPath)\images\icons\20 or $(ContentPath)\images\icons\16. 

This task outputs a png file for each size folder, and a single icons.css file at $(ContentPath)\css\icons.css.

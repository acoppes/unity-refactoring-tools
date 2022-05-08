[![openupm](https://img.shields.io/npm/v/com.gemserk.refactortools?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.gemserk.refactortools/) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
# Unity Refactor Tools

Tools for refactoring content stored in Unity Prefabs, Scenes and other Assets.

This project was initially used to show examples for this [blogpost](https://blog.gemserk.com/2022/04/24/refactoring-prefabs-and-unity-objects/).

### Install it from OpenUPM

This package can be installed using OpenUPM, just click here [![openupm](https://img.shields.io/npm/v/com.gemserk.refactortools?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.gemserk.refactortools/) for more details.

### Usage

If you want, for example, copy some MonoBehaviour fields to new field and consider all prefabs and prefab instances in scene, then run something like this:

```csharp
RefactorTools.RefactorMonoBehaviour<CustomBehaviour>(new RefactorTools.RefactorParameters
{
    considerScenes = true
}, delegate(GameObject gameObject, 
    RefactorTools.RefactorData _)
{
    var behaviours = gameObject.GetComponentsInChildren<CustomBehaviour>();
    foreach (var behaviour in behaviours)
    {
        behaviour.speed = new Speed
        {
            baseValue = behaviour.speedBaseValue,
            incrementValue = behaviour.speedIncrementValue
        };
    }
    return new RefactorTools.RefactorResult
    {
        completed = true
    };
});
```

### RefactorData parameter

You can customize refactors by using extra data received when refactoring with `RefactorData` parameter:

```csharp
RefactorTools.RefactorMonoBehaviour<CustomBehaviour>(new RefactorTools.RefactorParameters
{
    considerScenes = true
}, delegate(GameObject gameObject, 
    RefactorTools.RefactorData data)
{
    if (data.inScene)
    {
        if (data.scenePath.Contains("Levels"))
        {
            // do extra logic like search for specific reference or anything else
        }
    }
    else
    {
        // do some prefab refactor   
    }
});
```

### Roadmap

Here are some ideas on how to expand the project.

* Customize generic refactor logic by expanding `RefactorParameters`, `RefactorData` and `RefactorResult`.
* More refactor methods for general usage, like moving MonoBehaviour to another object, etc. 
* Customizable window to perform general refactors.
* Downgrade to Unity 2019.x or so.
* Automatize refactors using SerializedObjects and SerializedProperties to refactor field changes.
### Collaborate

Feel free to create issues for feature requests and/or submit pull requests with your own improvements. 

### Contact

<a href="https://twitter.com/intent/tweet?screen_name=arielsan&ref_src=twsrc%5Etfw"><img src="screenshots/twitter_logo.png" width="48"/><br/>@arielsan</a>
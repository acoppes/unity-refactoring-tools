# 0.0.14

* Refactor methods for Components/MonoBehaviours work now with interfaces.

# 0.0.13

* New ReplaceScript refactor method, to change from one MonoBehaviour to another.
* DestroyMonoBehaviour refactor receives optional refactor parameters now too.

# 0.0.12

* Display progress bar for find prefabs.
* Added API to find assets with specific text in the name.

# 0.0.11

* Show user save modified scenes dialog before refactoring (if considering scenes).

# 0.0.10

* Added general result for prefabs refactor to show failed prefabs (when missing scripts for example) and scenes.

# 0.0.9

* Now FindPrefabs<T> can search for components in inactive children too.

# 0.0.8

* Added support for FindAssets(type) instead of just generics.

# 0.0.7

* Check scenes and prefab parameters are not null.
* Fixed bug with search filter with FindAssets() 

# 0.0.6

* Fixed to sort by root prefab first when refactoring.

# 0.0.5

* Changed to pass prefabs and scenes as parameters.

# 0.0.4 

* Downgraded to Unity 2019.4.x.

# 0.0.3

* Changing to have a ParameterObject to declare how refactor should behave.
* Added RefactorResult struct to return after the refactor to allow the generic logic to do more stuff (in the future).
* Added more info in RefactorData.

# 0.0.2

* Added general refactor to remove MonoBehaviour from prefabs and scenes, and optinally destroy the empty GameObject in case there are no more components.

# 0.0.1 

* Initial version
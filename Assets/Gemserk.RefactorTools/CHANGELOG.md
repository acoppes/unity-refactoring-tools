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
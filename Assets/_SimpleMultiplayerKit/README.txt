Preparation:
Ensure that 'Run in Background' is enabled in the Player Settings
(Edit -> Project Settings -> Player -> Resolution and Presentation)

This is required for testing, as network signals won't be sent if the window isn't focused otherwise.

If using the example scenes, ensure '_Loader' scene is the first scene (0) in the Build Settings, with ExampleScene as the
	second scene (1). If another scene is to be used, ensure it is set appropriately in the scripts, since levels are
	called specifically through Application.LoadLevel(int).

After changing these, you can simply Build and Run the game, run multiple instances, and test it out for yourself!
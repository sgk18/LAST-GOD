import os

files = {
    'layer1': r'c:\projects\LAST-GOD\Assets\Art\Backgrounds\Layer1_FarBackground.png.meta',
    'layer2': r'c:\projects\LAST-GOD\Assets\Art\Backgrounds\Layer2_Midground.png.meta',
    'layer3': r'c:\projects\LAST-GOD\Assets\Art\Backgrounds\Layer3_Foreground.png.meta',
    'aeron_sprite': r'c:\projects\LAST-GOD\Assets\Characters\Aeron\Sprites\Idle\Aeron_Idle_01.png.meta',
    'aeron_controller': r'c:\projects\LAST-GOD\Assets\Characters\Aeron\Animator\Aeron.controller.meta',
    'camera_follow': r'c:\projects\LAST-GOD\Assets\Scripts\CameraFollow.cs.meta',
    'player_controller': r'c:\projects\LAST-GOD\Assets\Scripts\Player\PlayerController.cs.meta',
    'health': r'c:\projects\LAST-GOD\Assets\Scripts\Core\Health.cs.meta',
    'aeron_idle_ctrl': r'c:\projects\LAST-GOD\Assets\Characters\Aeron\Scripts\AeronIdleController.cs.meta',
    'parallax_mgr': r'c:\projects\LAST-GOD\Assets\Scripts\Core\ParallaxRuntimeManager.cs.meta',
    'parallax_layer': r'c:\projects\LAST-GOD\Assets\Scripts\Core\ParallaxLayer.cs.meta',
    'cutscene_ui': r'c:\projects\LAST-GOD\Assets\Scripts\Core\CutsceneUIController.cs.meta',
    'hud': r'c:\projects\LAST-GOD\Assets\Scripts\Player\PlayerControlsHUD.cs.meta'
}

for k, v in files.items():
    if os.path.exists(v):
        with open(v, 'r') as f:
            for line in f:
                if line.startswith('guid:'):
                    guid = line.strip().split()[1]
                    print(f'{k}: {guid}')
                    break
    else:
        print(f'{k} NOT FOUND')

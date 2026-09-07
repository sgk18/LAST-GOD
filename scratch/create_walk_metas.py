import os, hashlib

META_TEMPLATE = """fileFormatVersion: 2
guid: GUID_PLACEHOLDER
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 1
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    mipBias: 0
    wrapU: 0
    wrapV: 0
    wrapW: 0
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 9
  spritePivot: {x: 0.5, y: 0.022727}
  spritePixelsToUnits: 100
  spriteBorder: {x: 0, y: 0, z: 0, w: 0}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    format: -1
    textureCompression: 0
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

DIRS = [
    r"c:\projects\LAST-GOD\Assets\Characters\Aeron\Sprites\Walk",
    r"c:\projects\LAST-GOD\LAST-GOD\Assets\Characters\Aeron\Sprites\Walk"
]

for d in DIRS:
    os.makedirs(d, exist_ok=True)
    parent = os.path.dirname(d)
    walk_dir_meta = os.path.join(parent, "Walk.meta")
    if not os.path.exists(walk_dir_meta):
        h = hashlib.md5(b"Walk_Folder_Guid_Aeron").hexdigest()
        with open(walk_dir_meta, "w", encoding="utf-8") as f:
            f.write(f"fileFormatVersion: 2\nguid: {h}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {{}}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n")

for i in range(1, 9):
    fname = f"Aeron_Walk_{i:02d}.png"
    guid = hashlib.md5(f"Aeron_Walk_v1_{i:02d}".encode('utf-8')).hexdigest()
    content = META_TEMPLATE.replace("GUID_PLACEHOLDER", guid)
    
    for d in DIRS:
        p = os.path.join(d, fname + ".meta")
        with open(p, "w", encoding="utf-8") as f:
            f.write(content)
        print(f"Wrote {p} (guid: {guid})")

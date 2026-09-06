import os
import uuid
import hashlib

BASE_DIRS = [
    r"c:\projects\LAST-GOD\Assets",
    r"c:\projects\LAST-GOD\LAST-GOD\Assets"
]

def make_guid(name: str) -> str:
    return hashlib.md5(f"lastgod_thirdperson_{name}".encode('utf-8')).hexdigest()

def ensure_meta_files():
    for base in BASE_DIRS:
        tp_dir = os.path.join(base, "Scripts", "ThirdPerson")
        if not os.path.exists(tp_dir):
            continue
        
        # Meta for ThirdPerson directory
        meta_path = tp_dir + ".meta"
        if not os.path.exists(meta_path):
            guid = make_guid("dir_ThirdPerson")
            with open(meta_path, "w", encoding="utf-8") as f:
                f.write(f"fileFormatVersion: 2\nguid: {guid}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {{}}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n")
            print(f"Created {meta_path}")

        for root, dirs, files in os.walk(tp_dir):
            for d in dirs:
                dir_full = os.path.join(root, d)
                d_meta = dir_full + ".meta"
                if not os.path.exists(d_meta):
                    rel = os.path.relpath(dir_full, base).replace('\\', '/')
                    guid = make_guid(f"dir_{rel}")
                    with open(d_meta, "w", encoding="utf-8") as f:
                        f.write(f"fileFormatVersion: 2\nguid: {guid}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {{}}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n")
                    print(f"Created {d_meta}")

            for file in files:
                if file.endswith(".cs"):
                    file_full = os.path.join(root, file)
                    f_meta = file_full + ".meta"
                    if not os.path.exists(f_meta):
                        rel = os.path.relpath(file_full, base).replace('\\', '/')
                        guid = make_guid(f"script_{rel}")
                        with open(f_meta, "w", encoding="utf-8") as f:
                            f.write(f"fileFormatVersion: 2\nguid: {guid}\nMonoImporter:\n  externalObjects: {{}}\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {{instanceID: 0}}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n")
                        print(f"Created {f_meta}")

        # Meta for Act1OriginSceneBuilder.cs
        builder_cs = os.path.join(base, "Scripts", "Editor", "Act1OriginSceneBuilder.cs")
        builder_meta = builder_cs + ".meta"
        if os.path.exists(builder_cs) and not os.path.exists(builder_meta):
            guid = make_guid("script_Act1OriginSceneBuilder")
            with open(builder_meta, "w", encoding="utf-8") as f:
                f.write(f"fileFormatVersion: 2\nguid: {guid}\nMonoImporter:\n  externalObjects: {{}}\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {{instanceID: 0}}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n")
            print(f"Created {builder_meta}")

if __name__ == "__main__":
    ensure_meta_files()

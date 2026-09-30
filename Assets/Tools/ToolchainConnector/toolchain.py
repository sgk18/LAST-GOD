import os
import sys
import subprocess
import socket
import json
import time
from pathlib import Path

# Paths
ASEPRITE_EXE = r"C:\projects\aseprite\build\bin\aseprite.exe"
BLENDER_EXE = r"C:\Program Files\Blender Foundation\Blender 5.2\blender.exe"
UNITY_EXE = r"C:\Program Files\Unity\Hub\Editor\6000.5.8f1\Editor\Unity.exe"
PROJECT_DIR = r"c:\projects\LAST-GOD"

class AsepriteConnector:
    """Connector for Aseprite CLI and automation."""
    def __init__(self, exe_path=ASEPRITE_EXE):
        self.exe_path = exe_path
        if not os.path.exists(self.exe_path):
            raise FileNotFoundError(f"Aseprite executable not found at: {self.exe_path}")

    def create_aseprite_project(self, frame_paths, output_ase_path, fps=8):
        """Packs a list of image frame files into a native .aseprite project file."""
        os.makedirs(os.path.dirname(output_ase_path), exist_ok=True)
        cmd = [self.exe_path, "-b"] + [str(p) for p in frame_paths] + ["--save-as", str(output_ase_path)]
        res = subprocess.run(cmd, capture_output=True, text=True)
        if res.returncode != 0:
            raise RuntimeError(f"Aseprite error: {res.stderr}\nOutput: {res.stdout}")
        return os.path.exists(output_ase_path)

    def export_sheet(self, ase_path, output_sheet_path, output_json_path=None, sheet_type="horizontal"):
        """Exports a spritesheet from an existing .aseprite file."""
        cmd = [self.exe_path, "-b", str(ase_path), "--sheet-type", sheet_type, "--sheet", str(output_sheet_path)]
        if output_json_path:
            cmd += ["--data", str(output_json_path), "--format", "json-array"]
        res = subprocess.run(cmd, capture_output=True, text=True)
        if res.returncode != 0:
            raise RuntimeError(f"Aseprite sheet export error: {res.stderr}")
        return os.path.exists(output_sheet_path)

class BlenderConnector:
    """Connector for Blender headless execution and socket server."""
    def __init__(self, exe_path=BLENDER_EXE, host="localhost", port=9876):
        self.exe_path = exe_path
        self.host = host
        self.port = port

    def is_server_listening(self):
        """Checks whether the Blender MCP socket server is listening."""
        try:
            with socket.create_connection((self.host, self.port), timeout=1.0):
                return True
        except (socket.timeout, ConnectionRefusedError, OSError):
            return False

    def run_python_code(self, python_code, blend_file=None, timeout=60):
        """Executes arbitrary Python code inside Blender in headless mode."""
        import tempfile
        with tempfile.NamedTemporaryFile("w", suffix=".py", delete=False) as f:
            f.write(python_code)
            script_path = f.name
        
        cmd = [self.exe_path, "-b"]
        if blend_file and os.path.exists(blend_file):
            cmd += [str(blend_file)]
        cmd += ["-P", script_path]
        
        try:
            res = subprocess.run(cmd, capture_output=True, text=True, timeout=timeout)
            return {
                "success": res.returncode == 0,
                "stdout": res.stdout,
                "stderr": res.stderr,
                "code": res.returncode
            }
        finally:
            if os.path.exists(script_path):
                os.remove(script_path)

class UnityConnector:
    """Connector for Unity Editor batchmode and verification."""
    def __init__(self, exe_path=UNITY_EXE, project_path=PROJECT_DIR):
        self.exe_path = exe_path
        self.project_path = project_path

    def run_batchmode(self, execute_method, timeout=120):
        """Executes a static C# method in Unity batchmode with nographics."""
        log_path = os.path.join(self.project_path, "Temp", "batchmode_exec.log")
        os.makedirs(os.path.dirname(log_path), exist_ok=True)
        cmd = [
            self.exe_path,
            "-batchmode",
            "-nographics",
            "-projectPath", self.project_path,
            "-executeMethod", execute_method,
            "-logFile", log_path,
            "-quit"
        ]
        res = subprocess.run(cmd, capture_output=True, text=True, timeout=timeout)
        log_content = ""
        if os.path.exists(log_path):
            with open(log_path, "r", encoding="utf-8", errors="replace") as f:
                log_content = f.read()
        return {
            "success": res.returncode == 0,
            "log": log_content,
            "code": res.returncode
        }

if __name__ == "__main__":
    print("Testing Custom Toolchain Connectors...")
    ase = AsepriteConnector()
    print(f"Aseprite: Ready at {ase.exe_path}")
    
    blender = BlenderConnector()
    print(f"Blender: Ready at {blender.exe_path}, Server listening: {blender.is_server_listening()}")
    
    unity = UnityConnector()
    print(f"Unity: Ready at {unity.exe_path}")
    print("Toolchain Connector initialized successfully.")

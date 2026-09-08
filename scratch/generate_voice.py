import subprocess
import wave
import numpy as np
import os
import shutil

ps_code = """
Add-Type -AssemblyName System.Speech
$synth = New-Object System.Speech.Synthesis.SpeechSynthesizer
$synth.Rate = -2
$synth.Volume = 100
$synth.SetOutputToWaveFile('c:/projects/LAST-GOD/scratch/raw_voice.wav')
$synth.Speak('Wake up, Aeron... Awaken your power.')
$synth.Dispose()
"""

with open("scratch/run_synth.ps1", "w") as f:
    f.write(ps_code)

subprocess.run(["powershell", "-ExecutionPolicy", "Bypass", "-File", "scratch/run_synth.ps1"], check=True)

with wave.open("scratch/raw_voice.wav", "rb") as wf:
    params = wf.getparams()
    nchannels, sampwidth, framerate, nframes = params[:4]
    frames = wf.readframes(nframes)
    audio = np.frombuffer(frames, dtype=np.int16).astype(np.float32)

delay_samples = int(framerate * 0.18)
decay = 0.40
delay_samples2 = int(framerate * 0.36)
decay2 = 0.22

out_audio = np.zeros(len(audio) + delay_samples2 * 3, dtype=np.float32)
out_audio[:len(audio)] += audio
out_audio[delay_samples:delay_samples + len(audio)] += audio * decay
out_audio[delay_samples2:delay_samples2 + len(audio)] += audio * decay2

max_val = np.max(np.abs(out_audio))
if max_val > 0:
    out_audio = (out_audio / max_val) * 28000.0

out_bytes = out_audio.astype(np.int16).tobytes()

dest1 = "Assets/Audio/AudioClips/aeron_wakeup_voice.wav"
with wave.open(dest1, "wb") as wf:
    wf.setnchannels(nchannels)
    wf.setsampwidth(sampwidth)
    wf.setframerate(framerate)
    wf.writeframes(out_bytes)

dest2 = "LAST-GOD/Assets/Audio/AudioClips/aeron_wakeup_voice.wav"
if os.path.exists("LAST-GOD/Assets/Audio/AudioClips"):
    shutil.copyfile(dest1, dest2)

print("Generated voice line successfully:", dest1)

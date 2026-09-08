
Add-Type -AssemblyName System.Speech
$synth = New-Object System.Speech.Synthesis.SpeechSynthesizer
$synth.Rate = -2
$synth.Volume = 100
$synth.SetOutputToWaveFile('c:/projects/LAST-GOD/scratch/raw_voice.wav')
$synth.Speak('Wake up, Aeron... Awaken your power.')
$synth.Dispose()

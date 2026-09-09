from PIL import Image

a = Image.open('Assets/Art/Reference/Characters/Aeron_Turnaround.png')
g = Image.open('Assets/Art/Reference/Characters/Guard_Turnaround.png')

# Aeron Front
# Front figure roughly x from 100 to 570, y from 110 to 720
a_front = a.crop((100, 110, 580, 720))
a_front.save('Assets/Art/Reference/Characters/Aeron_Front.png')

# Aeron Side
a_side = a.crop((680, 110, 920, 720))
a_side.save('Assets/Art/Reference/Characters/Aeron_Side.png')

# Guard Front
# Guard is wider: front x from 60 to 560, y from 80 to 740
g_front = g.crop((60, 80, 560, 740))
g_front.save('Assets/Art/Reference/Characters/Guard_Front.png')

# Guard Side
g_side = g.crop((580, 80, 880, 740))
g_side.save('Assets/Art/Reference/Characters/Guard_Side.png')

print("Cropped views saved successfully!")

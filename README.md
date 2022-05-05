# A robot containing front-end in unity for interactive machine learning training
This interface is part of an imagined speech BCI where the person has an EEG cap and imagines the words dsplayed on the screen and the collected brain signal data of these words us used to train a machine learning model. The backend is available in another repository, https://github.com/svagatt/IML_backend. The premise of the ui interface is a simulated warehouse assembly unity with a helper robot so the words displayed are context relevant. The actual demo with all the assets can be seen here, https://vimeo.com/706543482.

This project contains,
- A robot prefab and display screen prefab
- A robot that tries to move towards the selected target object(Stochastic Gradient Algorithm, inspired from Alan Zuchonni's Inverse Kinematics tutorial https://www.alanzucconi.com/2017/04/10/robotic-arms/)
-Communication to connect to the backend(Python) server using NetMQ, multiple client sockets in the script Requesters.cs

# Robot Prefab
The robot prefab and muliple joint which move in different axes using inverse kinematics

# Display screen Prefab
The display screen prefab cotains a display scrren on the wall that generates a random sequence of words and its instructions. They can be seen in the scripts IterationSequenceList.cs and IterationDataPasser.cs.

# Idea
The interaction starts with user pressing the space key on the keyboard and scene changing to a fixation cross for a couple of seconds(where the user is supposedly imagining one of the three words), then that data is sent to the backend where the model predicts the label and relays it to the front end. The user provide label correction on a feedback prompt scene, then the scene switches, back to the warehouse. This code was originally written for this purpose but many of the scripts are reusable or offer a guidance into threasing and using sockets in unity, C#.

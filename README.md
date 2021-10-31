`Disclaimer:` I try to be discreet as much as possible about company but I might fail it at some point. ==Repository shall remain!==.

`SECOND IMPORTANT NOTE:` I use [Typora](https://typora.io/) for md file editing and I have a very customized setup for various MD format properties, like highlight, bold, italic text etc. if the text looks weird it is because of my custom CSS setup.

# CodeTest #8738

## Requirements

Write a C# console application which shall download and traverse a website and store it into local.

- use parallelism
- should download site recursively
- show progress
- parse links but can be simplified, with regex
- add various code quality aspects (tests, readibility, extendibility and so on)

app should be written within a few hours, not longer.

# Environment & Compilation

Application shall be in C#, NetCore3.1 (work environment). 

- open in VS2019 (my work environment) or another tool
- update nuget packages
- compile

executing

- run app
- enter path, website, thread count
- wait for end

# Design

Application shall use multithreading with *System.Threading.Tasks.Task* elements

Application shall use console to show progress (? need to learn how to update console ?)

Application shall cancel when clicked on a button while downloading stuff (?)

## Design Elements and Threads

APP shall have a list of files to download. also APP shall designate a bunch of threads ready to work, waiting for files to be enlisted.

APP shall at first download first index.html and then parse links. this process shall be single thread part. after parsing links it shall enlist more files to download (?dynamic increasing progress?)

APP threads shall try to fetch a file from download list and then download it and then parse links inside the file, to enlist more files to download. any thread can download any file, not stricted on a single tree branch.

APP shall stop when all files are downloaded

APP shall take website from console, also worker thread count as parameter. storage is also entered with parameter

## Nice To Have

Progress bar? (it is required)

Cancel on enter (it is not mentioned/required)

# Final Changes Compared to Above

Below are some final last minute changes

- Cancel on enter added. but not working %100. leaving as is
- Progress bar looks weird because of the files I refuse to download. some files/URIs require work to transform them into windows env but I opted not to spend time on it so progress bar at first shall increase but then decrease.
- storage is not entered from console but works at *Environment.CurrentDirectory* and creates a folder called storage. 
- a log file is used for internal logging since console is tricky to interact during progress updates

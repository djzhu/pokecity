#!/bin/sh

rm -rf AWXcode/*

/Applications/Unity2018/Unity.app/Contents/MacOS/Unity -quit -batchmode -executeMethod AutoBuilder.Build -projectPath ~/Documents/AdvanceWars/src/AWGame -logFile dist.log

filename=AWXcode-$(date +%Y%m%d.%H%M%S).zip

zip -r $filename AWXcode/

#rm -rf ../common/Xcode/*
#svn del ../common/Xcode/*
#cp $filename ../common/Xcode

#say "Build Complete!"
afplay CS.mp3



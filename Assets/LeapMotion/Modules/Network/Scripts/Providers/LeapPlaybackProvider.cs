using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Leap.Unity.Networking {
  public class LeapPlaybackProvider : LeapEncodingProvider {
    public LeapServiceProvider LeapDataProvider;
    public FrameEncodingEnum FrameEncodingType;

    public bool recording = false;
    private bool prevRecording = false;
    FileStream RecordingFile;
    public float fps = 15f;
    float lastUpdate = 0f;
    int recordedFrames = 0;

    public bool playing = false;
    private bool prevPlaying = false;
    //int playBackIndex = 0;
    int filePlayBufferBeginIndex = 0;
    int filePlayBufferEndIndex = 0;
    byte[] playBackBuffer = new byte[1048576]; //This is one megabyte; make this as small as necessary (but not smaller than one frame)
    //float lastPUpdate = 0f;
    public float currentBufferTime;
    float currentFileTime;
    //float totalFileTime;

    float timeRecording = 0f;

    FileStream PlayBackFile;
    String PlayBackFileFullName;
    int fileIndex = 0;
    int fileLength = 0;
    int fileNumberofFrames = 0;
    [HideInInspector]
    public FrameEncoding playerState;
    [HideInInspector]
    public FrameEncoding beginInterpolate;
    [HideInInspector]
    public FrameEncoding endInterpolate;
    private String fileName;

    int frameSize = 170;
    int headerLength = 8;
    String rootDirectory;
    public List<String> Recordings = new List<String>();

    void StartRecording() {
      if (RecordingFile == null) {
        fileName = rootDirectory + "/Save" + System.DateTimeOffset.Now.Millisecond + ".bin";
        RecordingFile = File.Create(fileName);
        RecordingFile.Write(BitConverter.GetBytes(playerState.frameSize), 0, 4);
        RecordingFile.Write(BitConverter.GetBytes(1 / fps), 0, 4);
        timeRecording = Time.time;
        recordedFrames = 0;
      }
    }

    void StartPlaying(String fullName) {
      if (PlayBackFile == null) {
        //Load the header and the file metadata
        byte[] header = new byte[headerLength];
        PlayBackFile = File.OpenRead(fullName);
        PlayBackFile.Read(header, 0, headerLength);
        frameSize = BitConverter.ToInt32(header, 0);
        fps = 1f / BitConverter.ToSingle(header, 4);
        FileInfo playingFile = new FileInfo(fullName);
        fileNumberofFrames = (((int)playingFile.Length - headerLength) / frameSize);
        fileIndex = headerLength;
        fileLength = (int)playingFile.Length;
        //totalFileTime = (float)fileNumberofFrames / fps;

        Debug.Log("Now playing: " + playingFile.Name + " - " + fileLength + " bytes - " + frameSize + " bytes per frame - " + Mathf.Round(1f / fps * 100f) / 100f + "ms per frame - " + fileNumberofFrames + " frames - " + (float)fileNumberofFrames * 1f / fps + " seconds long");

        readNextChunk();
      }
    }

    void StopPlaying() {
      if (PlayBackFile != null) {
        PlayBackFile.Close();
        PlayBackFile.Dispose();
        PlayBackFile = null;
      }
    }

    void Record() {
      if (Time.time > lastUpdate + (1 / fps)) {
        RecordingFile.Write(currentState.data, 0, currentState.data.Length);
        lastUpdate = Time.time;
        recordedFrames++;
      }
    }

    void readNextChunk() {
      currentBufferTime = 0f;

      //Choose to start over, or to advance the recording by another playBackBuffer's worth of data
      if (fileIndex >= fileLength) {
        fileIndex = headerLength;
        filePlayBufferBeginIndex = headerLength;
        filePlayBufferEndIndex = headerLength;
        currentFileTime = 0f;
      }

      int remainingNumberofFrames = (fileLength - fileIndex) / frameSize;

      //Read up to the end of the playBackBuffer or to the end of the file (whichever is smaller)
      int count = Math.Min((playBackBuffer.Length / frameSize) * frameSize, remainingNumberofFrames * frameSize);

      PlayBackFile.Seek(fileIndex, SeekOrigin.Begin);
      PlayBackFile.Read(playBackBuffer, 0, count);
      filePlayBufferBeginIndex = fileIndex;
      fileIndex += count;
      filePlayBufferEndIndex = fileIndex;
      //playBackIndex = 0;
    }

    //int fileTimeToIndex(float fileTime) {
    //  float totalFileTime = fileNumberofFrames / fps;
    //  return (int)((float)fileNumberofFrames * (fileTime / fileNumberofFrames / fps));
    //}

    void bufferTimeToFramesandAlpha(float bufferTime, out int startFrame, out float alphaToNextFrame) {
      int bufferFrames = ((filePlayBufferEndIndex - filePlayBufferBeginIndex) / frameSize);
      float bufferLength = (float)bufferFrames / fps;

      float decimalFrame = (float)bufferFrames * (bufferTime / bufferLength);

      startFrame = (int)decimalFrame;
      alphaToNextFrame = (decimalFrame - (float)startFrame);
    }

    void Play() {
      //If we have reached the end of our playBackBuffer or the file...
      if (currentBufferTime >= (float)((playBackBuffer.Length / frameSize) - 1) / fps || currentFileTime >= (float)(((fileLength - headerLength) / frameSize) - 1) / fps) {
        readNextChunk();
      }
      int startFrame; float alpha;
      bufferTimeToFramesandAlpha(currentBufferTime, out startFrame, out alpha);

      Array.Copy(playBackBuffer, startFrame * frameSize, beginInterpolate.data, 0, frameSize);
      beginInterpolate.fillEncoding(beginInterpolate.data);
      Array.Copy(playBackBuffer, (startFrame + 1) * frameSize, endInterpolate.data, 0, frameSize);
      endInterpolate.fillEncoding(endInterpolate.data);

      playerState.lerp(beginInterpolate, endInterpolate, alpha);

      currentBufferTime += Time.deltaTime/100f;
      currentFileTime += Time.deltaTime / 100f;
      AddFrameState(playerState);
    }

    void StopRecording() {
      if (RecordingFile != null) {
        //So while we intended to record samples at a certain rate, we're sampling within a sampling system
        //So let's just reformat our deltaTime to match the real world time that passed...
        timeRecording = Time.time - timeRecording;
        float interval = timeRecording / recordedFrames;
        RecordingFile.Seek(4, SeekOrigin.Begin);
        RecordingFile.Write(BitConverter.GetBytes(interval), 0, 4);
        RecordingFile.Close();
        RecordingFile.Dispose();
        RecordingFile = null;
        GetFilesInDirectory(rootDirectory);
      }
    }

    public override void Start() {
      base.Start();
      rootDirectory = Application.dataPath + "/../SavedRecordings/";
      switch (FrameEncodingType) {
        case FrameEncodingEnum.VectorHand:
          playerState = new VectorFrameEncoding();
          beginInterpolate = new VectorFrameEncoding();
          endInterpolate = new VectorFrameEncoding();
          frameSize = 170;
          break;
        case FrameEncodingEnum.CurlHand:
          playerState = new CurlFrameEncoding();
          beginInterpolate = new CurlFrameEncoding();
          endInterpolate = new CurlFrameEncoding();
          frameSize = 32;
          break;
        default:
          playerState = new VectorFrameEncoding();
          beginInterpolate = new VectorFrameEncoding();
          endInterpolate = new VectorFrameEncoding();
          frameSize = 170;
          break;
      }
      GetFilesInDirectory(rootDirectory);
    }

    void GetFilesInDirectory(String directory) {
      // Make a reference to a directory.
      DirectoryInfo di = new DirectoryInfo(directory);
      // Get a reference to each file in that directory.
      FileInfo[] fiArr = di.GetFiles();
      // Display the names and sizes of the files.
      //Debug.Log("The directory " + di.Name + " contains the following files:");
      byte[] header = new byte[headerLength];
      int size = 0; float interval = 0f;
      Recordings.Clear();
      for (int i = 0; i < fiArr.Length; i++) {
        if (fiArr[i].Extension.Equals(".bin")) {
          FileStream curFile = File.OpenRead(fiArr[i].FullName);
          curFile.Read(header, 0, headerLength);
          size = BitConverter.ToInt32(header, 0);
          interval = BitConverter.ToSingle(header, 4);
          Recordings.Add(fiArr[i].Name + " - " + fiArr[i].Length + " bytes - " + size + " bytes per frame - " + Mathf.Round(interval * 100f) / 100f + "ms per frame - " + ((fiArr[i].Length - headerLength) / size) + " frames - " + (float)((fiArr[i].Length - headerLength) / size) * interval + " seconds long");
          //Debug.Log(Recordings[Recordings.Count-1]);
          curFile.Close();
        }
      }
    }


    public override void Update() {
      if (prevRecording != recording) {
        if (recording) {
          StartRecording();
          playing = false;
        } else {
          Record();
          StopRecording();
        }
        prevRecording = recording;
      }

      if (prevPlaying != playing) {
        if (playing) {
          StartPlaying(fileName);
          recording = false;
        } else {
          StopPlaying();
        }
        prevPlaying = playing;
      }

      if (!playing) {
        playerState.fillEncoding(LeapDataProvider.CurrentFrame, transform);
        AddFrameState(playerState);
      } else {
        Play();
      }

      if (recording) {
        Record();
      }

      fillCurrentFrame(currentState);
      DispatchUpdateFrameEvent(CurrentFrame);
    }

    void OnDisable() {
      StopPlaying();
      StopRecording();
    }

    void OnApplicationQuit() {
      StopPlaying();
      StopRecording();
    }
  }
}
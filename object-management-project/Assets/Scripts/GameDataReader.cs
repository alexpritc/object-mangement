using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GameDataReader
{
   private BinaryReader reader;
   
   public int Version { get; }
   
   public GameDataReader(BinaryReader reader, int version)
   {
      this.reader = reader;
      Version = version;
   }
   
   public float ReadFloat()
   {
      return reader.ReadSingle();
   }

   public int ReadInt()
   {
      return reader.ReadInt32();
   }

   public Quaternion ReadQuaternion()
   {
      Quaternion value;
      value.x = ReadFloat();
      value.y = ReadFloat();
      value.z = ReadFloat();
      value.w = ReadFloat();
      return value;
   }

   public Vector3 ReadVector3()
   {
      Vector3 value;
      value.x = ReadFloat();
      value.y = ReadFloat();
      value.z = ReadFloat();
      return value;
   }
   
   public Color ReadColor()
   {
      Color value;
      value.r = ReadFloat();
      value.g = ReadFloat();
      value.b = ReadFloat();
      value.a = ReadFloat();
      return value;
   }

   public Random.State ReadRandomState()
   {
      return JsonUtility.FromJson<Random.State>(reader.ReadString());
   }
}

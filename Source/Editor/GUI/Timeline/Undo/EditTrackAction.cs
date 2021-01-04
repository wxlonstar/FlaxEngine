// Copyright (c) 2012-2021 Wojciech Figat. All rights reserved.

using System.IO;
using FlaxEngine;

namespace FlaxEditor.GUI.Timeline.Undo
{
    class EditTrackAction : IUndoAction
    {
        private Timeline _timeline;
        private string _name;
        private byte[] _beforeData;
        private byte[] _afterData;

        public EditTrackAction(Timeline timeline, Track track, byte[] beforeData, byte[] afterData)
        {
            _timeline = timeline;
            _name = track.Name;
            _beforeData = beforeData;
            _afterData = afterData;
        }

        public static byte[] CaptureData(Track track)
        {
            using (var memory = new MemoryStream(512))
            using (var stream = new BinaryWriter(memory))
            {
                stream.Write(track.Color);
                stream.Write(track.Mute);
                stream.Write(track.Loop);
                track.Archetype.Save(track, stream);
                return memory.ToArray();
            }
        }

        private void Set(byte[] data)
        {
            var track = _timeline.FindTrack(_name);
            using (var memory = new MemoryStream(data))
            using (var stream = new BinaryReader(memory))
            {
                track.Color = stream.ReadColor();
                track.SetMute(stream.ReadBoolean());
                track.Loop = stream.ReadBoolean();
                track.Archetype.Load(Timeline.FormatVersion, track, stream);
            }
            _timeline.ArrangeTracks();
            _timeline.MarkAsEdited();
            track.OnUndo();
        }

        public string ActionString => "Edit track";

        public void Do()
        {
            Set(_afterData);
        }

        public void Undo()
        {
            Set(_beforeData);
        }

        public void Dispose()
        {
            _timeline = null;
            _name = null;
            _beforeData = null;
            _afterData = null;
        }
    }
}

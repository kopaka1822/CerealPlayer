﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Annotations;
using CerealPlayer.Models.Hoster;
using CerealPlayer.Models.Task;
using Newtonsoft.Json;

namespace CerealPlayer.Models.Playlist
{
    public class PlaylistModel : INotifyPropertyChanged
    {
        private readonly IVideoHoster hoster;
        private readonly Models models;

        public struct SaveData
        {
            public VideoModel.SaveData[] Videos { get; set; }
            public string LastWebsite { get; set; }
            public string Name { get; set; }
            public int PlayingVideo { get; set; }
        }

        public PlaylistModel(Models models, string initialWebsite)
        {
            this.models = models;
            LastWebsite = initialWebsite;
            DownloadPlaylistTask = new DownloadPlaylistTask(this);

            // get series info
            hoster = models.Web.VideoHoster.GetCompatibleHoster(initialWebsite);
            Name = hoster.GetInfo(initialWebsite).SeriesTitle;
            System.IO.Directory.CreateDirectory(Directory);

            // create video and download task
            Videos.Add(new VideoModel(models, initialWebsite, this, hoster));

            var task = new NextEpisodeTaskModel(this);
            var subTask = hoster.GetNextEpisodeTask(task, initialWebsite);
            if (subTask != null)
            {
                task.SetNewSubTask(subTask);
            }
            this.NextEpisodeTask = task;
        }

        public PlaylistModel(Models models, SaveData data)
        {
            this.models = models;
            LastWebsite = data.LastWebsite;
            DownloadPlaylistTask = new DownloadPlaylistTask(this);

            // get series info
            hoster = models.Web.VideoHoster.GetCompatibleHoster(LastWebsite);
            Name = data.Name;

            // restore video tasks
            foreach (var video in data.Videos)
            {
                Videos.Add(new VideoModel(models, this, hoster, video));
            }

            playingVideoIndex = data.PlayingVideo;

            // restore next episode task
            var task = new NextEpisodeTaskModel(this);
            var subTask = hoster.GetNextEpisodeTask(task, data.LastWebsite);
            if (subTask != null)
            {
                task.SetNewSubTask(subTask);
            }
            this.NextEpisodeTask = task;
        }

        public void AddNextEpisode(string website)
        {
            LastWebsite = website;
            Videos.Add(new VideoModel(models, website, this, hoster));
        }

        public string Name { get; }

        public string LastWebsite { get; private set; }

        public string Directory => models.App.PlaylistDirectory + "/" + Name;

        private int playingVideoIndex = 0;

        public int PlayingVideoIndex
        {
            get => playingVideoIndex;
            set
            {
                if (value >= Videos.Count) return;
                if(value < 0) return;
                
                if (playingVideoIndex == value) return;
                playingVideoIndex = value;
                OnPropertyChanged(nameof(PlayingVideoIndex));
            }
        }

        public ObservableCollection<VideoModel> Videos = new ObservableCollection<VideoModel>();

        [NotNull]
        public DownloadPlaylistTask DownloadPlaylistTask { get; }

        [NotNull]
        public NextEpisodeTaskModel NextEpisodeTask { get; }

        public string SettingsLocation => GetSettingsLocation(Directory);

        public static string GetSettingsLocation(string directory)
        {
            return directory + "/cereal.json";
        }

        public static PlaylistModel LoadFromDirectory(string directory, Models models)
        {
            var json = File.ReadAllText(GetSettingsLocation(directory));
            var data = JsonConvert.DeserializeObject<SaveData>(json);
            return new PlaylistModel(models, data);
        }

        public SaveData GetSaveData()
        {
            var vs = new List<VideoModel.SaveData>();
            foreach (var videoModel in Videos)
            {
                vs.Add(videoModel.GetSaveData());
            }

            return new SaveData
            {
                Videos = vs.ToArray(),
                LastWebsite = LastWebsite,
                Name = Name,
                PlayingVideo = PlayingVideoIndex
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

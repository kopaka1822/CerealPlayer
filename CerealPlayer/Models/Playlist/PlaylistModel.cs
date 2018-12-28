using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
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
            public long PlayingPosition { get; set; }
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
            videos.Add(new VideoModel(models, initialWebsite, this, hoster));
            PlayingVideoIndex = 0;

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
                videos.Add(new VideoModel(models, this, hoster, video));
            }

            PlayingVideoIndex = data.PlayingVideo;
            PlayingVideoPosition = new TimeSpan(data.PlayingPosition);

            // restore next episode task
            var task = new NextEpisodeTaskModel(this);
            var subTask = hoster.GetNextEpisodeTask(task, data.LastWebsite);
            if (subTask != null)
            {
                task.SetNewSubTask(subTask);
            }
            this.NextEpisodeTask = task;
        }

        /// <summary>
        /// adds a new episode to the end of the list
        /// and sets LastWebsite to website.
        /// Additionally sets active video if it was null
        /// </summary>
        /// <param name="website"></param>
        public void AddNextEpisode([NotNull] string website)
        {
            LastWebsite = website;
            videos.Add(new VideoModel(models, website, this, hoster));
            if (playingVideo == null)
            {
                PlayingVideo = videos.Last();
            }
        }

        /// <summary>
        /// deletes a video from the list and chooses another playing video if
        /// this was the playing video
        /// </summary>
        /// <param name="video"></param>
        public void DeleteEpisode([NotNull] VideoModel video)
        {
            Debug.Assert(videos.Contains(video));
            // is it the active video?
            if (ReferenceEquals(video, playingVideo))
            {
                // take the next episode
                if (PlayingVideoIndex < videos.Count - 1)
                {
                    PlayingVideoIndex++;
                }
                //take the previous episode
                else if(PlayingVideoIndex > 0)
                {
                    PlayingVideoIndex--;
                }
                // set active video to null
                else
                {
                    PlayingVideo = null;
                }
            }
            Debug.Assert(!ReferenceEquals(video, playingVideo));
            videos.Remove(video);
            // did the play index change?
            var prevPlayIndex = playingVideoIndex;
            RecalcPlaylingVideoIndex();
            if(prevPlayIndex != playingVideoIndex)
                OnPropertyChanged(nameof(PlayingVideoIndex));
        }

        /// <summary>
        /// name of the series
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// link of the last videos website
        /// </summary>
        public string LastWebsite { get; private set; }

        /// <summary>
        /// full directory path of the playlist
        /// </summary>
        public string Directory => models.App.PlaylistDirectory + "/" + Name;

        #region Active Video

        private VideoModel playingVideo = null;

        /// <summary>
        /// video that is currently played or should be played if the playlist is active.
        /// Can be null (if waiting for a new episode to be added)
        /// </summary>
        public VideoModel PlayingVideo
        {
            get => playingVideo;
            set
            {
                Debug.Assert(value == null || videos.Contains(value));
                if (ReferenceEquals(value, playingVideo)) return;

                playingVideo = value;
                var prevPlayingIndex = playingVideoIndex;
                RecalcPlaylingVideoIndex();

                // reset play position
                PlayingVideoPosition = TimeSpan.Zero;

                OnPropertyChanged(nameof(PlayingVideo));

                if (prevPlayingIndex != playingVideoIndex)
                    OnPropertyChanged(nameof(PlayingVideoIndex));
            }
        }

        /// <summary>
        /// The position of the playing video in TimeSpan ticks.
        /// This property is excluded from INotifyPropertyChanged.
        /// </summary>
        public TimeSpan PlayingVideoPosition { get; set; } = TimeSpan.Zero;

        // set to -1 to reset value in models contructor
        private int playingVideoIndex = -1;

        /// <summary>
        /// shows the index of the video that is currently played 
        /// or Videos.Count if the next episode that will be added should be played
        /// </summary>
        public int PlayingVideoIndex
        {
            get => playingVideoIndex;
            set
            {
                if (value > Videos.Count) return;
                if (value < 0) return;

                if (playingVideoIndex == value) return;
                playingVideoIndex = value;
                PlayingVideo = playingVideoIndex == Videos.Count ? null : videos[playingVideoIndex];
                OnPropertyChanged(nameof(PlayingVideoIndex));
            }
        } 

        #endregion

        #region Video Collection

        private readonly ObservableCollection<VideoModel> videos = new ObservableCollection<VideoModel>();

        public IReadOnlyCollection<VideoModel> Videos => videos;

        public event NotifyCollectionChangedEventHandler VideosCollectionChanged
        {
            add => videos.CollectionChanged += value;
            remove
            {
                if (value != null) videos.CollectionChanged -= value;
            }
        }

        #endregion

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
                PlayingVideo = PlayingVideoIndex,
                PlayingPosition = PlayingVideoPosition.Ticks
            };
        }

        /// <summary>
        /// sets the playing video index without rasing its changed event
        /// </summary>
        private void RecalcPlaylingVideoIndex()
        {
            if (playingVideo == null)
            {
                playingVideoIndex = videos.Count;
                return;
            }
            playingVideoIndex = videos.IndexOf(playingVideo);
            Debug.Assert(playingVideoIndex >= 0);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

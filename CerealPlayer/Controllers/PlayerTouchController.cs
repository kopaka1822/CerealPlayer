using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CerealPlayer.Controllers
{
    public class PlayerTouchController
    {
        private readonly Models.Models models;
        private readonly MediaElement player;
        private bool isTouching = false;
        private Point startPoint;
        private int touchId = 0;

        public PlayerTouchController(Models.Models models)
        {
            this.models = models;
            player = models.App.Window.Player;

            player.TouchDown += PlayerOnTouchDown;
            player.TouchMove += PlayerOnTouchMove;
            player.TouchUp += PlayerOnTouchUp;
        }

        private void PlayerOnTouchUp(object sender, TouchEventArgs e)
        {
            if (!isTouching || touchId != e.TouchDevice.Id) return;

            isTouching = false;
            var endPoint = e.TouchDevice.GetTouchPoint(player).Position;
            var diff = endPoint - startPoint;
            if (diff.X > 200.0)
            {
                if (models.Playlists.ActivePlaylist != null)
                    models.Playlists.ActivePlaylist.PlayingVideoIndex -= 1;
            }
            else if(diff.X < -200.0)
            {
                if (models.Playlists.ActivePlaylist != null)
                    models.Playlists.ActivePlaylist.PlayingVideoIndex += 1;
            }
        }

        private void PlayerOnTouchMove(object sender, TouchEventArgs e)
        {
            if(!isTouching || touchId != e.TouchDevice.Id) return;
            
            var endPoint = e.TouchDevice.GetTouchPoint(player).Position;
        }

        private void PlayerOnTouchDown(object sender, TouchEventArgs e)
        {
            if (isTouching) return;

            isTouching = true;
            startPoint = e.GetTouchPoint(player).Position;
            touchId = e.TouchDevice.Id;
        }
    }
}

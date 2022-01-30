using Godot;
using System;

public class MainMenu : Control
{
    private AudioStreamPlayer audioPlayer;
    private AudioStreamSample menuSong;

    public override void _Ready()
    {
        menuSong = GD.Load<AudioStreamSample>("res://Sounds/MainMenu.wav");
        audioPlayer = GetNode<AudioStreamPlayer>("AudioPlayer");
        audioPlayer.Autoplay = true;
        audioPlayer.Stream = menuSong;
        audioPlayer.Play();
    }

    public void _on_StartButton_pressed()
    {
        GetTree().ChangeScene("res://Scenes/LevelHolder.tscn");
    }

    public void _on_QuitButton_pressed()
    {
        GetTree().Quit();
    }
}

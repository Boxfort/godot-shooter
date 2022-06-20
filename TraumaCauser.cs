using Godot;
using System;

public class TraumaCauser : Area
{
    public override void _Ready()
    {
        
    }

    public void CauseTrauma(float amount) 
    {
        var areas = GetOverlappingAreas();
        foreach (Area @area in areas) 
        {
            if (@area is ShakeableCamera camera) {
                camera.AddTrauma(amount);
            }
        }
    }
}

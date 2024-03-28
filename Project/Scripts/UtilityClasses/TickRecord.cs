using Godot;
using System;

public class TickRecord
{
    const int len = 10;
    ulong[] time_data = new ulong[len];
    int index = 0;
    public void Append(ulong new_time)
    {
        time_data[index] = new_time;
        index += 1;
        if(index >= len) index = 0;
    }
    public double GetAverage()
    {
        ulong acc = 0;
        for(int q = 0; q < len; q++) acc += time_data[q];
        return acc / len;
    }
}
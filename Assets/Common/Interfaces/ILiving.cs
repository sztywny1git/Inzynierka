using System;
public interface ILiving
{
    bool isAlive { get; }
    event Action Death;
}
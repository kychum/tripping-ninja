using System;

public struct Course
{
    int id;
    Dept department;
    int coureNum;
    string name;
    string desc;
    string prof;
    ClassType type;
    short days;         // Encoded in bits. _SFTWTMS (could be reversed if desired)
    short startTime;
    short duration;     // Number of hours
}

enum Dept
{
    ART = 0,
    BIOL,
    BSEN,
    CPSC,
    EDUC,
    ENGG,
    ENGL,
    MATH,
    MUSI,
    SENG,
};

enum ClassType
{
    Lecture = 0,
    Tutorial,
    Laboratory,
};

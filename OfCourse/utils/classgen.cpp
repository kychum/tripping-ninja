/**
 * Used to generate random classes.
 */
#include <iostream>
#include <fstream>
#include <string>
#include <cstdlib>
#include <ctime>
#include <ostream>
using namespace std; //bite me

#define SUN 1
#define MON 2
#define TUE 4
#define WED 8
#define THU 16
#define FRI 32
#define SAT 64
#define VERSION "1.3"

enum Faculty{
	Arts = 0,
	Business,
	Education,
	Engineering,
	//EnvironmentalDesign,
	//GraduateStudies,
	//Law,
	//Nursing,
	//Medicine,
	Science,
	//SocialWork
};

enum Dept{
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

string Departments[] = {
	"Art",
	"Biology",
	"Business and Environment",
	"Computer Science",
	"Education",
	"Engineering",
	"English",
	"Math",
	"Music",
	"Software Engineering",
};

enum ClassType{
	Lecture = 0,
	Tutorial,
	Laboratory,
};

string ClassTypes[] = {
	"Lecture",
	"Tutorial",
	"Laboratory",
};

string Profs[] = { // Randomly generated names
	"Stacey Manning", "Ivan Harrington", "Jacqueline Tate", "Nina Hudson", "Nelson Knight",
	"Julian Hardy", "Audrey Morton", "Vicki Woods", "Roxanne Hansen", "Tamara Mason",
	"Mattie Hogan", "Lydia Bowen", "Mae Dunn", "Clark Mccarthy", "Simon Osborne",
	"Conrad Diaz", "Johanna Wood", "Elijah Barrett", "Felicia Davis", "Clarence Crawford",
	"Vincent James", "Owen Reed", "Angie Reid", "Catherine Abbott", "Preston Webb",
	"Jimmy Owen", "Molly Cohen", "Garry Copeland", "Olga Stewart", "Courtney Smith",
	"Alonzo Douglas", "Elsa Malone", "Timothy Jenkins", "Tara Howard", "Wendy Thornton",
	"Guadalupe Gomez", "Darnell Phillips", "Arturo Miles", "Jack Ramos", "Diana Steele",
	"Nathan Gordon", "Theodore Larson", "Jordan Allison", "Tomas Bryant", "Edmund Wallace",
	"Mercedes Fields", "Steve Peters", "Alma Kelly", "Jeannie Romero", "Taylor Oliver",
};

string CNames[] = {
	"Introduction to ",
	"Applications of ",
	"Advanced Topics in ",
	"Special Topics in ",
	"Work Term in ",
	"Theory of ",
};

struct Course{
	int id;
	Dept department;
	int courseNum;
	string name;
	string desc;
	string prof;
	ClassType type;
	unsigned short days; // use the first 7 bits
	unsigned short startTime; // 00 - 23 ?? Probably restrict between 7 and 5
	unsigned short duration; // Assume exact hours, between 1-3.
	string prereqs; // Dept.Num; ORs denote by ||, AND by commas
	string antireqs; // comma-delimited
	short enrollable; // 0: open 1: closed 2: waitlist

	friend ostream& operator<<(ostream& o, const Course& c){
		o << c.id << "\n" << c.department << "\n" << c.courseNum << "\n" << c.name+"\n" << c.desc+"\n" << c.prof+"\n" << c.type << "\n" << c.days << "\n" << c.startTime << "\n" << c.duration << "\n" << c.prereqs << "\n" << c.antireqs << "\n" << c.enrollable << endl;
		return o;
	}
};

int main(int argc, char** argv){
	if(argc != 2){
		cerr << "Usage: " << argv[0] << " <Number of courses to generate>\n";
		return 1;
	}
	srand(time(NULL));
	int outAmt = atoi(argv[1]);

	cout << VERSION << endl;// << outAmt << endl; // I'm lazy, actually. just redirect the stream
	for(int i = 0; i < outAmt; i++){
		Course c;
		c.id = i;
		c.department = static_cast<Dept>(rand()%10);
		c.courseNum = (((rand()%5)+1)*100) + (rand()%3);
		c.name = CNames[(c.courseNum/100)-1]+Departments[c.department]+" "+to_string(c.courseNum%100+1);
		c.desc = c.name;
		c.prof = Profs[rand()%50];
		c.type = Lecture;
		c.days = rand() & 62; //msvc doesn't accept binary literals //0b00111110; // Let's just assume M-F
		c.startTime = (rand()%10)+7;
		c.duration = rand()%3 + 1; // Up to 3 hours long
		
		//prereq logic (Should really make more sense but we just need something that works for now
		if(c.courseNum >= 200){
			int numReqs = rand() % 5; // Up to 5 prereqs for now.
			for(int r = 0; r < numReqs; r++){
				c.prereqs += to_string(rand()%10) + "." + to_string((((rand()%5)+1)*100) + (rand()%3));
				if(r+1 < numReqs){
					c.prereqs += (rand()%2 == 0 ? "," : "|");
				}
			}
		}
		else{
			c.prereqs="";
		}

		//antireq logic
		int numAnti = rand() % 3; // Let's say 2 antireqs. Just to test things out
		for(int a = 0; a < numAnti; a++){
				c.antireqs += to_string(rand()%10) + "." + to_string((((rand()%5)+1)*100) + (rand()%3));
				if(a+1 < numAnti){
					c.antireqs += ",";
				}
		}

		c.enrollable = (rand()%100 < 75 ? 0 : rand()%2+1);

		cout << c;

		// Add tutorials and labs
		int numLinked = rand()%3;
		if(numLinked > 0 && numLinked+i < outAmt){ // The idea is to force enrollment in these when the main lecture is selected.
			for(int j = 0; j < numLinked; j++){
				c.id++;
				c.prof = Profs[rand()%50];
				c.type = static_cast<ClassType>((rand()%2)+1);
				c.days = rand() & 62;//0b00111110; // sigh
				if(c.days == 0)
					c.days = 26; // default to MTF
				c.startTime = (rand()%10)+7;
				c.duration = rand()%3+1;
				if(c.enrollable == 0)
					c.enrollable = rand()%2;
				else // waitlisted? or closed?
					c.enrollable = 1; // closed
				cout << c;
			}
		}
	}

	return 0;
}

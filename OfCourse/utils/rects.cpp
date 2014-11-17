// Use if the borders need to be remade/recoloured whatever
// I'm not sure if VS has a better way to do this.

#include <cstdio>
using namespace std;

char* colour = "#FFC9C9C9";
char* fmt = "<Border Grid.Column=\"%d\" Grid.Row=\"%d\" BorderThickness=\"1,1,%d,%d\" BorderBrush=\"%s\"/>\n";

int main(){
	for(int col = 0; col < 6; col++){
		for(int row = 0; row < 13; row++){
			bool d = false, r=false;
			if(col == 5)
				r = true;
			if(row == 12)
				d = true;
			printf(fmt,col,row,r,d,colour);
		}
	}	
	return 0;
}

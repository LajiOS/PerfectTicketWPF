#include <stdio.h>
#include <stdlib.h>
#include <windows.h>
#include <time.h>
#define STATIONE_NUM 10
#define USER_NUM 10
#define MAX_TICKET 30

char username[10][10] = {"sfhka\0","dskgfa\0","trhfw\0","fiste\0","duise\0","dtiuo\0","njgur\0","fjvgey\0","dxjfr\0","vhegu\0"};
int hash[10][10] = {0};
int main(void) {
    FILE *t_table;
    FILE *u_table;
    //...
    t_table = fopen("ticket_table.txt", "w");
    u_table = fopen("user_table.txt", "w");
    //...
    int t_id = 1;
    for(;t_id <= 100;t_id ++)
    {
    	srand((unsigned)(GetCurrentProcessId() + t_id));
    	int s_stt = rand()%STATIONE_NUM;
    	srand((unsigned)(time(NULL)) + t_id);
    	int t_ett = rand()%STATIONE_NUM;
    	srand((unsigned)(GetCurrentProcessId()) + time(NULL) + t_id);
    	int sold = rand()%2;
    	int o_id = 0;
    	if(sold)
    	{
    		srand(sold + t_id);
    		o_id = rand()%USER_NUM;
    	}
    	else
    	{
    		o_id = 0;
    	}
    	if(hash[s_stt][t_ett] <= MAX_TICKET){
    		hash[s_stt][t_ett] ++;
	    	fprintf(t_table, "%d    %d    %d    %d    %d\n", t_id,s_stt,t_ett,sold,o_id);
    	}
    	else{
    		t_id --;
    	}
    }

    int u_id = 1;
    for(;u_id <= 10;u_id ++)
    {
    	srand((unsigned)(GetCurrentProcessId()) + u_id);
    	int u_psw = rand()%1000000;
    	srand((unsigned)(time(NULL)) + u_id);
    	int u_pri = 1 << (rand()%3);
    	srand((unsigned)(GetCurrentProcessId()) + time(NULL) + u_id);
    	int u_bal = 10*(rand()%10)+100;
    	fprintf(u_table, "%d    %s    %d    %d    %d\n", u_id,username[u_id-1],u_psw,u_pri,u_bal);
    }
    //...
    return 0;
}
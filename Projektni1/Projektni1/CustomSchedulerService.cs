using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

/*  @author: Bosko Gogic
*   @version: .NET Core 3.0 
*/

namespace ProjektniJedan
{
    /* Proizvoljno napravljeni Task Scheduler*/
    class CustomSchedulerService : TaskScheduler
    {
        /**  atribut za specifikaciju broja niti u proizvoljnom rasporedjivacu*/
        private int brojNiti;
        
        /**  atribut za specifikaciju ogranicenja vremena izvrsavanja za svaku od 
         *  proslijedjenih funkcija proslijedjenih rasporedjivacu za rasporedjivanje
         */
        private int vrijemeIzvrsavanja;

        /**  Atributi koje je potrebno imati za overrajdovanje metoda */
        private BlockingCollection<Task> tasksCollection = new BlockingCollection<Task>();
        private readonly Thread mainThread = null;
        

        /**  Atribut koji reprezentuje niz Zadataka - Taskova */
        private static CustomSchedulerService _instance;
        private List<Timer> timers = new List<Timer>();

        private Action task;
        private int hour;
        private int min;
        private int sec;
        private int milisec;
        private int prioritet;

        /** Konstruktor */
        public CustomSchedulerService(int hour, int min, int sec, int milisec, int prioritet, Action task)
        {
            this.hour = hour;
            this.min = min;
            this.sec = sec;
            this.milisec = milisec;
            this.prioritet = prioritet;
            this.task = task;
        }

        /*  Konstruktor kopije*/
        public CustomSchedulerService(CustomSchedulerService cs)
        {
            this.hour = cs.hour;
            this.min = cs.min;
            this.sec = cs.sec;
            this.milisec = cs.milisec;
            this.prioritet = cs.prioritet;
            this.task = cs.task;

        }


        public void SchedulerByTime(int sec)
        {

            TimeSpan timeToGo = new TimeSpan(0, 0, 0, this.sec, this.milisec);
            task.Invoke();
            Thread.Sleep(sec * 1000);
        }

        public int getPrioritet()
        {
            return prioritet;
        }

        public void setPrioritet(int prioritet)
        {
            this.prioritet = prioritet;
        }

        public Action getTask()
        {
            return task;
        }


        public int getSec()
        {
            return sec;
        }

        public void setSec(int sec)
        {
            this.sec = sec;
        }

        public void preventivnoRasporedjivanje(CustomSchedulerService t1, String nazivPrvog, CustomSchedulerService t2, String nazivDrugog)
        {
            /** Priority Ceiling Protocol -> Kada zadatak(task) A viseg prioriteta pokusa 
             *  zauzeti resurs koji je vec zauzeo zadatak B koji je  nizeg prioriteta, 
             *  njegov prioritet se dodjeli zadatku(task) - u B koji vec koristi taj resurs 
             *  a taj zadatak(task) A sto je htio zauzeti resurs ceka.
             *  Nakon sto zadatak B zavrsi sa koristenjem resursa prioriteti se vracaju 
             *  na orginalnu postavku
             */
            int prioritet1 = t1.getPrioritet();
            int prioritet2 = t2.getPrioritet();
            int prioritet = 0;

            Console.WriteLine("===============================================\n");
            if(t1.getPrioritet() > t2.getPrioritet())
            {
                t1.setSec((t1.getSec() / 2));
                t1.SchedulerByTime(t1.getSec());
                Console.WriteLine("Task {1} (Prioritet {0}) radi u kriticnoj sekciji!\n ", t1.getPrioritet(), nazivPrvog);
                Console.WriteLine("Task {1} (Prioritet {0}) zeli da pristupi resursu u kriticnoj sekciji!\n", t2.getPrioritet(), nazivDrugog);
         
                Thread.Sleep(2000);
                Console.WriteLine("\nSlijedi izmjena prioriteta!");
                /** Izmjeni prioritete !*/
                prioritet = t1.getPrioritet();
                t1.setPrioritet(t2.getPrioritet());
                t2.setPrioritet(prioritet);
                Console.WriteLine("Novi prioritet od task {1} je {0} \n", t1.getPrioritet(), nazivPrvog);
                Console.WriteLine("Novi prioritet od task {1} je {0} \n", t2.getPrioritet(), nazivDrugog);
                t1.SchedulerByTime(t1.getSec());
                
            }
            else
            {
                t1.setSec((t1.getSec() / 2));
                t1.SchedulerByTime(t1.getSec());
                Console.WriteLine("Task {1} (Prioritet {0}) radi u kriticnoj sekciji!\n ", t1.getPrioritet(), nazivPrvog);
                Console.WriteLine("Task {1} (Prioritet {0}) zeli da pristupi resursu u kriticnoj sekciji!\n", t2.getPrioritet(), nazivDrugog);

                Thread.Sleep(2000);
                Console.WriteLine("Task {1} (Prioritet {0}) ima veci prioritet pa ne dolazi do izmjene prioriteta!", t1.getPrioritet(), nazivPrvog);
            }
            Console.WriteLine("===============================================\n");

            
            t1.setPrioritet(prioritet1);
            t2.setPrioritet(prioritet2);

            Console.WriteLine("Nakon preventivnog rasporedjivanja, prioriteti su orginalni -> za {0} je {1} a za {2} je {3}", nazivPrvog, t1.getPrioritet(), nazivDrugog, t2.getPrioritet());

        }

        public CustomSchedulerService[] sortiraj(CustomSchedulerService[] niz)
        {
            /** Dio koda koji sortira niz po prioritetu */
            int i = 0;
            int j = 0;
            for (i = 0; i < 5; i++)
            {
                for (j = i + 1; j < 5; j++)
                {
                    if (niz[i].getPrioritet() > niz[j].getPrioritet())
                    {
                        CustomSchedulerService pomocni = new CustomSchedulerService(0, 0, 10, 3, 0, niz[i].getTask());
                        pomocni = niz[i];
                        niz[i] = niz[j];
                        niz[j] = pomocni;
                    }
                }
            }
            return niz;
        }


        
        


        /** Metode koje se moraju override kada nasljedjujemo Task Scheduler */
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return tasksCollection.ToArray();
        }
        protected override void QueueTask(Task task)
        {
            if (task != null)
                tasksCollection.Add(task);
        }
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }

        /** Demonstrativne funkcije za prezentovanje rada aplikacije */
        public void saberi(int a, int b)
        {
            Console.WriteLine("Zbir dva broja je : {0}", a + b);
        }

        public void parnost(int a)
        {
            if(a % 2 == 0)
            {
                Console.WriteLine("Broj {0} je paran!",a);
            }
            else
            {
                Console.WriteLine("Broj {0} je neparan!", a);
            }
        }

        public void stepenovanje(int a, int stepen)
        {
            int broj = a;
            int stepenPomocni = stepen;
            while(stepen != 1)
            {
                a = a*broj;
                stepen--;
            }
            Console.WriteLine("Broj {0} na stepen {1} je broj {2}", broj, stepenPomocni, a);
        }

        public void kvadriraj(int a)
        {
            int broj = a;
            a *= a;
            Console.WriteLine("Kvadriran broj {0} je {1} ", broj, a);
        }

        public void oduzmi(int a, int b)
        {
            int razlika = a - b;
            Console.WriteLine("Razlika broja {0} i broja {1} je {2}", a, b, razlika);
        }
    }
}

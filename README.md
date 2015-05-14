##Prospector demo

This is a demonstration of a web crawler that identifies hacked sites that are unknowingly serving spam. In this example, the program will find sites which sell the prescription medication Cialis. The crawler can target any keyword or combination of keywords, as well as filter by gTLD.

Positive results are indicated by the repetitive occurrence of the keyword in many successive search engine results. For example, a site which has 'Cialis' in every title, URL and snippet accross 20+ pages is 95% of the time a hacked site. 

###Installation

`git clone` to your local machine and open the .sln file in Visual Studio 2012 or later. Debug to build and run

###Output

The program will provide a list of compromised sites. Information for each site includes two URLs; one for the home page and another for the first encountered piece of spam. A screenshot of the spam page is also included.

In the past I have used this information to construct automated outreach emails. In the future I may set up a public-service notification system. 
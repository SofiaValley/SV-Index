import re
import json
import urllib
import os

class Crawler():
    URL = "http://it.jobs.bg/front_job_search.php?frompage={0}&str_regions=&str_locations=&tab=jobs&old_country=&country=-1&region=0&l_category%5B%5D=0&keyword=#paging"
    Step = 20
    hrefpattern = "href=\"f\d+\""
    idpattern = "f\d+"
    jobsbg = "http://it.jobs.bg/"
    filename = "hrefs.txt"
    offers = "Offers"

    def Run(self):
        haspages = True
        index = 0
        offers = list()
        
        while(haspages):
            pageoffers = list()
            url = self.URL.format(index)
            response = urllib.urlopen(url)
            hrefs = re.findall(self.hrefpattern, response.read())
            for href in hrefs:
                pageoffers.append(href)
            index += 20
            haspages = len(pageoffers) > 0
            offers.extend(pageoffers)
        
        with open(self.filename, "w") as out:
            out.truncate()
            result = json.dumps(offers)
            out.write(result)
   
    def DownloadOffers(self):
        f = open(self.filename, "r")
        hrefs = list(json.loads(f.read()))
        for href in hrefs:
            pageid = re.search(self.idpattern, href).group(0)
            url = self.jobsbg + pageid
            response = urllib.urlopen(url).read()
            with open(os.path.join(self.offers, pageid), "w+") as out:
                out.write(response)
 


class Parser():
      languages = [("\bjava\b(?!\s*script)", "Java"),
      ("\bc\#", "C#"),
      ("\bvb\b", "VisualBasic"),
      ("\bvisual\s*basic\b", "VisualBasic"),
      ("\bc\s*\+\+", "C++"),
      ("(?!\bobjective)\bc(?!(#\+))\b", "C"),
      ("\bphp\b", "PHP"),
      ("\bpython\b", "Python"),
      ("\bruby\b", "Ruby"),
      ("\bobjective(\s-)*c\b", "ObjectiveC"),
      ("\bjava\s*script\b", "JavaScript"),
      ("\bdelphi\b", "Delphi")]

      def Parse(self):
        print(self.languages)

#c = Crawler()
#c.Run()
#c.DownloadOffers()

p = Parser()
p.Parse();
import re
import json
import urllib
import os

class Config():
    URL = "http://it.jobs.bg/front_job_search.php?frompage={0}&str_regions=&str_locations=&tab=jobs&old_country=&country=-1&region=0&l_category%5B%5D=0&keyword=#paging"
    Step = 20
    hrefpattern = "href=\"f\d+\""
    idpattern = "f\d+"
    jobsbg = "http://it.jobs.bg/"
    filename = "hrefs.txt"
    offers = "Offers"

class Crawler():   
    def Run(self):
        haspages = True
        index = 0
        offers = list()
        
        while(haspages):
            pageoffers = list()
            url = Config.URL.format(index)
            response = urllib.urlopen(url)
            hrefs = re.findall(Config.hrefpattern, response.read())
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
        f = open(Config.filename, "r")
        hrefs = list(json.loads(f.read()))
        for href in hrefs:
            pageid = re.search(Config.idpattern, href).group(0)
            url = Config.jobsbg + pageid
            response = urllib.urlopen(url).read()
            with open(os.path.join(Config.offers, pageid), "w+") as out:
                out.write(response)
 


class Parser():
      languages = [(r'\bjava\b(?!\s*script)', 'Java'),
      (r'\bc\#', 'C#'),
      (r'\bvb\b', 'VisualBasic'),
      (r'\bvisual\s*basic\b', 'VisualBasic'),
      (r'\bc\s*\+\+', 'C++'),
      (r'\b(?!objective)\bc(?![#+]+)\b', 'C'),
      (r'\bphp\b', 'PHP'),
      (r'\bpython\b', 'Python'),
      (r'\bruby\b', 'Ruby'),
      (r'\bobjective(\s-)*c\b', 'ObjectiveC'),
      (r'\bjava\s*script\b', 'JavaScript'),
      (r'\bdelphi\b', 'Delphi')]

      def Parse(self):
        result = dict([(lan, 0) for (pattern, lan) in self.languages])
        for file in os.listdir(Config.offers):
            with open(os.path.join(Config.offers, file), "r") as f:
                content = f.read()
                for (pattern, lan) in self.languages:
                    if(re.search(pattern, content, flags = re.LOCALE | re.IGNORECASE | re.MULTILINE)):
                        result[lan]+=1
                        break
        print(sorted(result.items(), key=lambda x: x[1]))                 
                       

#c = Crawler()
#c.Run()
#c.DownloadOffers()

p = Parser()
p.Parse();
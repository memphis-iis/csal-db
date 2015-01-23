"""post_reading.py

This simple script allows you to post a reading request for a student. Note
that this is really more of a test/demo script than a tool for production
usage.

You must run on the command line with three parameters:

    ./post_reading.py [options] endpoint student reading_url

where:

 * endpoint is where to post the data.  "Special" parameters are
   allowed: $Please_See_Endpoint_Specials

 * student is the subject ID for a student record - it is posted as UserID
 
 * reading_url is the URL to be saved - it is posted as TargetURL

The options allowed are:

 * -v verbose output, including the JSON that is posted

 * -r read back - all reading posted for the given user will be read back and
   written out
"""

import sys
import json
import urllib2


ENDPOINT_SPECIALS = {
    "test": "http://localhost:62702/api/studentreading",
    "prod": "http://ace.autotutor.com/csaldb/api/studentreading"
}
def xlate_endpoint(e):
    return ENDPOINT_SPECIALS.get(e, e)


def output(s, *args):
    if output.verbose:
        if args:
            s = s % args
        print s
output.verbose = False


def usage():
    es = '\n'.join([
        "    - %s becomes %s" % kv 
        for kv in ENDPOINT_SPECIALS.iteritems()
    ])
    print __doc__.replace("$Please_See_Endpoint_Specials", '\n' + es)
    return 1


def encode(s):
    """Perform URL/URI encoding per the scheme used in CSALMongoWebAPI for
    URL parameters"""
    return urllib2.quote(urllib2.quote(s, safe=''))


def do_get(url):
    #Actual get (read)
    output("GET: %s" % url)
    req = urllib2.Request(url)
    resptxt = urllib2.urlopen(req).read()
    if not resptxt:
        return None #Nothing found
    return json.loads(resptxt)

def do_post(url, data):
    #Actual post (write)
    output("POST: %s" % url)
    databody = json.dumps(data)
    output("POST-body: %s", databody)
    
    req = urllib2.Request(
        url,
        databody,
        {'Content-Type': 'application/json'}
    )
    
    resptxt = urllib2.urlopen(req).read()
    if resptxt:
        output("FOR REQ: %s %s", url, str(data))
        output("RESPONSE: %s", resptxt)
        raise ValueError("Response from server?!")


def main():
    args = sys.argv[1:]

    if len(args) < 3:
        return usage()
    
    parms = []
    read_back = False
    
    for arg in args:
        if arg.startswith('-'):
            optn = arg[1:]
            if   optn == 'v': output.verbose = True
            elif optn == 'r': read_back = True
            else: return usage()
        else:
            parms.append(arg)
    
    if len(parms) != 3:
        return usage()
    
    endpoint, userid, targeturl = parms
    endpoint = xlate_endpoint(endpoint)
    
    output("Endpoint:  %s", endpoint)
    output("UserID:    %s", userid)
    output("TargetURL: %s", targeturl)
    
    do_post(endpoint, {"UserID": userid, "TargetURL": targeturl})

    if read_back:
        rb_url = endpoint + '/' + encode(userid)
        entries = do_get(rb_url)
        output("Got %d entries", len(entries))
        for idx,entry in enumerate(entries):
            print "%4d [%s] %s" % (
                idx+1, 
                entry.get("VisitTime", "{Visit Time Missing}"), 
                entry.get("TargetURL", "{URL Missing}"), 
            )

if __name__ == "__main__":
    main()

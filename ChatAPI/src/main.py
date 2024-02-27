

# make sure KEY has been supplied
# exp use : api_key = os.environ["ANYSCALE_API_KEY"]
from fastapi import HTTPException, FastAPI, websockets

import json  # for JSON parsing and packing

# for easy async code
import asyncio

# for absolute project paths
import os

# astro chat engines
from chat_objects import *
from chat_engine import ChatEngine

from vedastro import *  # install via pip

import time  # for performance measurements


# load API keys from .env file
import os
if "ANYSCALE_API_KEY" not in os.environ:
    raise RuntimeError("KEY MISSING DINGUS")


FAISS_INDEX_PATH = "faiss_index"

# instances embedded vector stored here for speed, shared between all calls
loaded_vectors = {}
chat_engines = {}

# init app to handle HTTP requests
app = FastAPI(title="Chat API")


# ..𝕗𝕠𝕣 𝕚𝕗 𝕪𝕠𝕦 𝕒𝕣𝕖 𝕥𝕙𝕖 𝕨𝕠𝕣𝕤𝕥 𝕠𝕗 𝕨𝕠𝕣𝕤𝕥 
# 𝕒𝕟𝕕 𝕪𝕠𝕦 𝕝𝕠𝕧𝕖 𝔾𝕠𝕕, 𝕪𝕠𝕦❜𝕣𝕖 𝕗𝕣𝕖𝕖❕
# Yogananda

@app.get("/")
def home():
    return {"Welcome to VedAstro"}


# SEARCH
@app.post('/HoroscopeLLMSearch')
async def horoscope_llmsearch(payload: SearchPayload):
    try:
        global loaded_vectors

        # lazy load for speed
        # use file path as id for dynamic LLM modal support
        savePathPrefix = "horoscope"
        # use modal name for multiple modal support
        filePath = f"{FAISS_INDEX_PATH}/{savePathPrefix}/{payload.llm_model_name}"
        if loaded_vectors.get(filePath) is None:
            loaded_vectors[filePath] = EmbedVectors(filePath, payload.llm_model_name)# load the horoscope vectors (heavy compute)

        # # get all predictions for given birth time (aka filter)
        # run calculator to get list of prediction names for given birth time
        birthTime = payload.get_birth_time()
        calcResult = Calculate.HoroscopePredictionNames(birthTime)

        # format list nicely so LLM can swallow
        birthPredictions = {"name": [item for item in calcResult]}

        # do LLM search on found predictions
        results_formated = loaded_vectors[filePath].search(payload.query, payload.search_type, birthPredictions)
        return results_formated

    # if fail, fall gracefully
    except Exception as e:
        raise HTTPException(status_code=400, detail=str(e))


# RAG
@app.websocket("/HoroscopeChat")
async def horoscope_chat(websocket: websockets.WebSocket):
    global chat_engines # use cache
    global loaded_vectors # use cache

    await websocket.accept()
    await websocket.send_text("Welcome to VedAstro!")
    
    try:
        # connection has been made, now talk to client
        while True:
            # Receive a message from the client
            raw_data = await websocket.receive_text()
            
            # Check if the client wants to exit
            if raw_data == "":
                break
            
            # Parse the message
            payload = ChatPayload(raw_data)

            # Let caller process started
            await websocket.send_text("Thinking....")

            # STEP 1: GET NATIVE'S HOROSCOPE DATA (PREDICTIONS)
            # get all predictions for given birth time (aka filter)
            # run calculator to get list of prediction names for given birth time
            birth_time = payload.get_birth_time()
            calc_result = Calculate.HoroscopePredictionNames(birth_time)
            all_predictions = {"name": [item for item in calc_result]} # format list nicely so LLM can swallow (dict)


            # STEP 2: GET PREDICTIONS THAT RELATES TO QUESTION
            # load full vector DB (contains all predictions text as numbers)
            savePathPrefix = "horoscope"  # use file path as id for dynamic LLM modal support
            # use modal name for multiple modal support
            filePath = f"{FAISS_INDEX_PATH}/{savePathPrefix}/{payload.llm_model_name}"
            if loaded_vectors.get(filePath) is None:  # lazy load for speed
                # load the horoscope vectors (heavy compute)
                loaded_vectors[filePath] = EmbedVectors(filePath, payload.llm_model_name)
            # do LLM search on found predictions
            found_predictions = loaded_vectors[filePath].search(payload.query, payload.search_type, all_predictions)


            # STEP 3: COMBINE CONTEXT AND QUESTION AND SEND TO CHAT LLM
            # run QA prepare the LLM that will answer the query
            if chat_engines.get(filePath) is None:  # lazy load for speed
                wrapper = ChatEngine(payload.variation_name) # select the correct engine variation
                chat_engines[filePath] = wrapper.create_instance(payload.chat_model_name) # load the modal shards (heavy compute)

            # Query the chat engine and send the results to the client
            async for chunk in await chat_engines[filePath].query(query=payload.query,
                                    input_documents=found_predictions,
                                    # Controls the trade-off between randomness and determinism in the response
                                    # A high value (e.g., 1.0) makes the model more random and creative
                                    temperature=payload.temperature,
                                    # Controls diversity of the response
                                    # A high value (e.g., 0.9) allows for more diversity
                                    top_p=payload.top_p,
                                    # Limits the maximum length of the generated text
                                    max_tokens=payload.max_tokens,
                                    # Specifies sequences that tell the model when to stop generating text
                                    stop=payload.stop,
                                    # Returns debug data like usage statistics
                                    return_debug_data=False  # Set to True to see detailed information about model usage
                                    ):
                await websocket.send_text(chunk['output_text'])

    #Handle failed gracefully
    except Exception as e:
        print(e)


# REGENERATE HOROSCOPE EMBEDINGS
# takes all horoscope predictions text and converts them into LLM embedding vectors
# which will be used later to run queries for search & AI chat
@app.post('/HoroscopeRegenerateEmbeddings')
async def horoscope_regenerate_embeddings(payload: RegenPayload):
    from langchain_core.documents import Document

    # 1 : get all horoscope texts direct from VedAstro library
    horoscopeDataList = HoroscopeDataListStatic.Rows

    # repackage all horoscope data so LLM can understand (docs)
    docs = [Document(page_content=horoscope.Description, metadata={"name": horoscope.Name.ToString(
    ), "nature": horoscope.Nature.ToString()}) for horoscope in horoscopeDataList]

    # 2 : embed the horoscope texts, using CPU LLM (also saves to local disk under modal name)
    time_minutes = await ChatTools.TextChunksToEmbedingVectors(payload, docs, "horoscope")

    # tell call all went well
    return {"Status": "Pass",
            "Payload": f"Amen ✝️ complete, it took {time_minutes} min"}


# PRESET MATCH
@app.post("/PresetQueryMatch")
async def preset_query_match(payload: TempPayload):
    from local_huggingface_embeddings import LocalHuggingFaceEmbeddings
    import numpy as np
    from sklearn.metrics.pairwise import cosine_similarity

    # 2 : embed the horoscope texts, using CPU LLM
    embeddings = LocalHuggingFaceEmbeddings(payload.llm_model_name)

    preset_queries = [
    "health",
    "finance",
    "education",
    "romance",
    "travel",
    "state of mind"
    ]
    preset_vectors = embeddings.embed_documents(preset_queries)

    user_vector = embeddings.embed_query(payload.query)

    print(user_vector)

    # compares the user vector to the preset vectors to determine how similar they are
    user_vector_expanded = np.expand_dims(user_vector, axis=0) # Make the vectors match
    similarities = cosine_similarity(user_vector_expanded, preset_vectors)

    most_similar_query = preset_queries[np.argmax(similarities)]

    return {"result": most_similar_query}

# SERVER STUFF

# TRAINING
